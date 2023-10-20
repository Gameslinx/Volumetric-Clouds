float _StepSize;
float _SunStepSize;
float _Absorption;
float _Scattering;
float _DensityFactor;
float _DensityThreshold;
float _CloudBottomFadeDist;
#define EPSILON 0.0001
#define PI 3.141592654

#include "SimplexNoise.cginc"

float _CloudScale;
float _SphereCenter;
float _InnerSphereRadius;
float _OuterSphereRadius;
float _CoverageScale;
sampler3D _CloudTex;
sampler2D _CoverageMap;
sampler2D _CloudTypesMap;
sampler3D _DetailTex;
float2 _DetailTex_ST;
float _DetailScale;
float _Debug;
sampler2D _BlueNoise;

struct Ray
{
    float3 pos;
	float3 dir;
	float density;
};
float hash(float3 p3)
{
	p3  = frac(p3 * 0.1031);
    p3 += dot(p3, p3.zyx + 31.32);
	float rnd = frac((p3.x + p3.y) * p3.z);
    return lerp(0.4, 1, rnd);
}
float IGN(int x, int y)
{
    return (52.9829189 * (0.06711056 * (float)x + 0.00583715 * (float)y) % 1.0f) % 1.0f;
}
// Projects a point in 3D down to the _InnerSphereRadius, for sampling noise tiled across it#
// Only used for spherical clouds
float3 project(float3 pos)
{
	return 1;
}
// Return the true distance to the thick cloud plane
float PlaneSDF(float3 position, float3 dir)
{
	float verticalDist = abs(position.y	 - (_InnerSphereRadius + _OuterSphereRadius) * 0.5f) - (_OuterSphereRadius - _InnerSphereRadius) * 0.5f;
	float cosAngle = abs(dot(dir, float3(0, 1, 0)));
	float trueDist = verticalDist / cosAngle;
	if (verticalDist < EPSILON)
	{
		return verticalDist; 
	}
	return trueDist;
}
float SphereSDF(float3 position, float3 dir)
{
	return length(position) - 2;
	

	float dist2 = abs(length(position)-(_OuterSphereRadius + _InnerSphereRadius) / (_OuterSphereRadius - _InnerSphereRadius)) - _CloudBottomFadeDist;
	return dist2;
	if (dist2 < _StepSize && dist2 > EPSILON)
	{
		return _StepSize;
	}
	return dist2;

	float dist = length(position);
	
	float distToOuter = dist - _OuterSphereRadius;
	float distToInner = _InnerSphereRadius - dist;

	return max(distToOuter, distToInner);
	// Within hollow sphere
}
float Altitude01(float y)
{
	return (y - _InnerSphereRadius) / (_OuterSphereRadius - _InnerSphereRadius);
}
// Split into GetNoiseBase and GetNoiseDetail
float SampleDensity(float3 rayPos)
{
	// Retrieve cloud coverage (what type of cloud is at this location?) - Essentially splat mapping
	fixed4 coverageMap = tex2Dlod(_CoverageMap, float4(rayPos.zx * _CoverageScale, 0, 0));
	// Obtain the coverages for the cloud types we have
	float4 coverages = tex2Dlod(_CloudTypesMap, float4(0.5, Altitude01(rayPos.y), 0, 0));
	// Mix coverages based on the coverage map
	float coverage = min(1, coverageMap.r * coverages.r + coverageMap.g * coverages.g + coverageMap.b * coverages.b);
	// The simple representation of our clouds
	float4 base = tex3Dlod(_CloudTex, float4(rayPos * _CloudScale, 0));
	base = 0.3333 * (base.r * coverage + base.g + base.b);
	base = max(0, base - _DensityThreshold) * _DensityFactor;

	return base;
}
float SampleDetailedDensity(float3 rayPos)
{
	//float base = smoothstep(0, 1, 3 - length(rayPos)) * 4;

	float noise = tex3Dlod(_DetailTex, float4(rayPos * _DetailScale + (_Time.x % 1) * 1, 0));
	float noise2 = tex3Dlod(_DetailTex, float4(rayPos * _DetailScale * 2 + (_Time.x % 1) * 1.2, 0));

	noise = (noise * 2 + noise2) / 3.0f;
	noise = pow(noise + 0.5, 3);

	return noise;
	//float result = base * noise;
	//return result;
}
float Shlick(float cosAngle, float k)
{	
	return (1 - k * k) / (4 * PI * (1 + k * cosAngle) * (1 + k * cosAngle));
}
float Phase(float cosAngle, float g, float h, float k)
{
	return lerp(Shlick(cosAngle, g), Shlick(cosAngle, h), k);// (HenyeyGreenstein(cosAngle, g) * k + HenyeyGreenstein(cosAngle, h) * (1 - k)) * 0.5;
}
float LightMarch(Ray ray, int steps, float extinction)
{
	//float distFromSphere = SphereSDF(ray.pos, ray.dir);
	for (int y = 0; y < steps; y++)
	{
		float distFromSphere = PlaneSDF(ray.pos, ray.dir);
		if (distFromSphere < EPSILON)
		{
			ray.density += SampleDensity(ray.pos) * _StepSize;
			ray.pos += ray.dir * _StepSize;
		}
		else
		{
			ray.pos += ray.dir * distFromSphere;
		}
	}
	return exp(-ray.density * extinction * _Scattering);
}