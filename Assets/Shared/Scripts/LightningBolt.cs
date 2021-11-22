using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightningBolt : MonoBehaviour
{
    public class Particle
	{
        public float size = 1.5f;
        public Color color = Color.white;
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 velocity;
		public Quaternion angularVelocity;
	}
    
    public Transform target;
    public int zigs = 50;
    public float speed = 1.0f;
    public float scale = 1.0f;
    public float arcHeight = 0.0f;
    public bool depthSort = false;
    
    public float particleMinSize = 0.5f;
    public float particleMaxSize = 1.5f;
    public float particleSizeScale = 1.0f;

    MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    
    Perlin noise = new Perlin();
    Mesh mesh;
    
    List<int> renderOrder = new List<int>();
    List<Particle> particles = new List<Particle>();
	List<Vector3> vertices = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<Color> colors = new List<Color>();
    List<int> indices = new List<int>();
    
    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(meshRenderer.sharedMaterial);
        meshRenderer.enabled = false;
        
        mesh = new Mesh();
		mesh.MarkDynamic();
        meshFilter.sharedMesh = mesh;
        
        SetParticleCount(zigs);
    }
    
    void OnEnable() {
        meshRenderer.enabled = true;
    }
    
    void OnDisable() {
        meshRenderer.enabled = false;
    }
    
    // void OnValidate()
    // {
    //     meshFilter = GetComponent<MeshFilter>();
    //     meshRenderer = GetComponent<MeshRenderer>();
        
    //     mesh = new Mesh();
	// 	mesh.MarkDynamic();
    //     meshFilter.sharedMesh = mesh;
        
    //     SetParticleCount(zigs);
    // }
    
    public void SetParticleCount(int count)
	{
		if(count == 0)
		{
			mesh.Clear();
            renderOrder.Clear();
            particles.Clear();
            vertices.Clear();
            uvs.Clear();
            colors.Clear();
            indices.Clear();
			return;
		}
        
		renderOrder.Clear();
    	for(int i = 0; i < count; ++i)
			renderOrder.Add(i);
        
		particles.Clear();
		for(int i = 0; i < count; ++i)
        {
            var sz = Random.Range(particleMinSize, particleMaxSize);
			particles.Add(new Particle(){ size = sz });
        }
        
		int vertexCount = count * 4;
        
        vertices.Clear();
        vertices.Resize(vertexCount);
        uvs.Clear();
        uvs.Resize(vertexCount);
        colors.Clear();
        colors.Resize(vertexCount);
        indices.Clear();
        indices.Resize(vertexCount);
        
		Vector2 uvTopLeft  = new Vector2(0.0f, 1.0f);
		Vector2 uvBotLeft  = new Vector2(0.0f, 0.0f);
		Vector2 uvBotRight = new Vector2(1.0f, 0.0f);
		Vector2 uvTopRight = new Vector2(1.0f, 1.0f);
		
		for(int i = 0, p = 0; i < vertexCount; i += 4, ++p)
		{
			int i0 = i + 0;
			int i1 = i + 1;
			int i2 = i + 2;
			int i3 = i + 3;
            
			vertices[i0] = Vector3.zero;
            uvs[i0] = uvTopLeft;
			colors[i0] = Color.white;
            indices[i0] = i0;
            
			vertices[i1] = Vector3.zero;
            uvs[i1] = uvBotLeft;
			colors[i1] = Color.white;
            indices[i1] = i1;
            
			vertices[i2] = Vector3.zero;
            uvs[i2] = uvBotRight;
			colors[i2] = Color.white;
            indices[i2] = i2;
            
			vertices[i3] = Vector3.zero;
            uvs[i3] = uvTopRight;
			colors[i3] = Color.white;
            indices[i3] = i3;
		}
		
		mesh.Clear();
		mesh.SetVertices(vertices);
		mesh.SetUVs(0, uvs);
		mesh.SetColors(colors);
		mesh.SetIndices(indices.ToArray(), MeshTopology.Quads, 0);
        mesh.UploadMeshData(false);
        mesh.RecalculateBounds();
	}
    
    void LateUpdate()
    {
        if(particles.Count == 0)
			return;
		
        float now = Time.time;
        float timex = now * speed * 0.1365143f;
        float timey = now * speed * 1.21688f;
        float timez = now * speed * 2.5564f;
        float oneOverZigs = 1.0f / (float)zigs;
        
        for(int i = 0, sz = particles.Count; i < sz; ++i)
        {
            float strength = 1.0f - (float)i / (float)sz;
            
            Vector3 position = Vector3.Lerp(transform.position, target.position, oneOverZigs * (float)i);
            
            if(arcHeight > 0)
            {
                float t = oneOverZigs * (float)i;
                float arcVariance = Mathf.PerlinNoise(now * 5.0f, 0.5f + t * 3.0f) * arcHeight * 0.25f;
                position += Vector3.up * (arcHeight + arcVariance) * Curve.ArcQuad(t);
            }
            
            Vector3 offset = new Vector3(noise.Noise(timex + position.x, timex + position.y, timex + position.z),
                                         noise.Noise(timey + position.x, timey + position.y, timey + position.z),
                                         noise.Noise(timez + position.x, timez + position.y, timez + position.z)) * strength;

                                         
            position += (offset * scale * ((float)i * oneOverZigs));
            
            particles[i].position = position;
            particles[i].color = Color.white;
        }
        
		Vector3 camPos = Camera.main.transform.position;
        
		if(depthSort)
		{
            renderOrder.Sort((int x, int y)=> {
				Vector3 vx = (particles[x].position - camPos);
				Vector3 vy = (particles[y].position - camPos);
				return -vx.sqrMagnitude.CompareTo(vy.sqrMagnitude);
			});
		}
        
		Vector2 uvTopLeft  = new Vector2(0.0f, 1.0f);
		Vector2 uvBotLeft  = new Vector2(0.0f, 0.0f);
		Vector2 uvBotRight = new Vector2(1.0f, 0.0f);
		Vector2 uvTopRight = new Vector2(1.0f, 1.0f);
        
		for(int i = 0, p = 0, sz = vertices.Count;
		    i < sz;
		    i += 4, ++p)
		{
			int pi = depthSort ? renderOrder[p] : p;
            
			Vector3 pos 		= particles[pi].position;
			Quaternion rot 		= particles[pi].rotation;
			float size			= particles[pi].size * particleSizeScale;
			Vector3 vel 		= particles[pi].velocity;
			Quaternion angVel 	= particles[pi].angularVelocity;
			Color col           = particles[pi].color;
			
			pos += vel * Time.deltaTime;
			rot = Quaternion.Slerp(Quaternion.identity, angVel, Time.deltaTime) * rot;
			
            particles[pi].position = pos;
			particles[pi].rotation = rot;
			
			int i0 = i + 0;
			int i1 = i + 1;
			int i2 = i + 2;
			int i3 = i + 3;
			
			Vector3 topLeft  = new Vector3(-size * 0.5f,  size * 0.5f, 0);
			Vector3 botLeft  = new Vector3(-size * 0.5f, -size * 0.5f, 0);
			Vector3 botRight = new Vector3( size * 0.5f, -size * 0.5f, 0);
			Vector3 topRight = new Vector3( size * 0.5f,  size * 0.5f, 0);
			
            var localPos = transform.InverseTransformPoint(pos);
            var localFw = transform.InverseTransformDirection((camPos - pos).normalized);
            var localRot = Quaternion.LookRotation(localFw, Vector3.up);

            uvs[i0] = uvTopLeft;
            vertices[i0] = localPos + localRot * topLeft;
			colors[i0] = col;
            indices[i0] = i0;

            uvs[i1] = uvBotLeft;
            vertices[i1] = localPos + localRot * botLeft;
			colors[i1] = col;
            indices[i1] = i1;

            uvs[i2] = uvBotRight;
            vertices[i2] = localPos + localRot * botRight;
			colors[i2] = col;
            indices[i2] = i2;

            uvs[i3] = uvTopRight;
            vertices[i3] = localPos + localRot * topRight;
			colors[i3] = col;
            indices[i3] = i3;
        }

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
		mesh.SetColors(colors);
        mesh.SetIndices(indices, MeshTopology.Quads, 0, false);
        mesh.UploadMeshData(false);
        mesh.RecalculateBounds();
    }
}
