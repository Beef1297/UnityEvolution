using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Evolution
{
    public enum ProceduralModelingMaterial
    {
        Standard,
        UV,
        Normal,
    };

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public abstract class ProceduralModelingBase : MonoBehaviour
    {

        private MeshFilter filter;
        public MeshFilter Filter
        {
            get
            {
                if (filter == null)
                {
                    filter = GetComponent<MeshFilter>();
                }
                return filter;
            }
        }
        new MeshRenderer renderer; // ?? Why "new" is located in ahead
        public MeshRenderer Renderer
        {
            get
            {
                if (renderer == null)
                {
                    renderer = GetComponent<MeshRenderer>();
                }
                return renderer;
            }
        }
        [SerializeField] protected ProceduralModelingMaterial materialType = ProceduralModelingMaterial.UV;

        protected virtual void Start()
        {
            Rebuild();
        }

        public void Rebuild()
        {
            if (Filter.sharedMesh != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(Filter.sharedMesh); // What's sharedMesh!
                }
                else
                {
                    DestroyImmediate(Filter.sharedMesh);
                }
            }
            Filter.sharedMesh = Build();
            Renderer.sharedMaterial = LoadMaterial(materialType);
        }

        protected virtual Material LoadMaterial(ProceduralModelingMaterial type)
        {
            switch(type)
            {
                case ProceduralModelingMaterial.Normal:
                    return Resources.Load<Material>("Materials/Normal");
                case ProceduralModelingMaterial.UV:
                    return Resources.Load<Material>("Materials/UV");
            }
            return Resources.Load<Material>("Materials/Standard");
        }

        protected abstract Mesh Build();
    }
}
