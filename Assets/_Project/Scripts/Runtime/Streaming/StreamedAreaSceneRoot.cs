using UnityEngine;

namespace WhereTheSoundingsEnd.Streaming
{
    /// <summary>
    /// Marks the single inactive root of a static, independently streamed area scene.
    /// Streamed area scenes must not own gameplay scripts, save participants or rigidbodies:
    /// those remain in the persistent gameplay scene so their state and references survive unloads.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StreamedAreaSceneRoot : MonoBehaviour
    {
        [SerializeField] private string areaId = string.Empty;
        [SerializeField] private string contentRevision = "1";

        public string AreaId => areaId;
        public string ContentRevision => contentRevision;

        public void Configure(string stableAreaId, string revision)
        {
            areaId = stableAreaId;
            contentRevision = revision;
        }

        public bool TryValidate(string expectedAreaId, string expectedRevision, out string error)
        {
            if (gameObject.activeSelf)
            {
                error = "The streamed area root must be saved inactive so it cannot flash into view before validation.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(areaId) ||
                !string.Equals(areaId, expectedAreaId, System.StringComparison.Ordinal))
            {
                error = "Area identity mismatch. Expected '" + expectedAreaId + "', received '" + areaId + "'.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(contentRevision) ||
                !string.Equals(contentRevision, expectedRevision, System.StringComparison.Ordinal))
            {
                error = "Area content revision mismatch for '" + areaId + "'.";
                return false;
            }

            MonoBehaviour[] scripts = GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < scripts.Length; i++)
            {
                if (scripts[i] != null && scripts[i] != this)
                {
                    error = "Streamed area '" + areaId +
                            "' contains gameplay script '" + scripts[i].GetType().Name +
                            "'. Keep gameplay controllers in the persistent scene.";
                    return false;
                }
            }

            Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>(true);
            if (bodies.Length > 0)
            {
                error = "Streamed area '" + areaId +
                        "' contains a Rigidbody. Dynamic gameplay bodies must remain in the persistent scene.";
                return false;
            }

            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i] != null && meshFilters[i].sharedMesh == null)
                {
                    error = "A MeshFilter in streamed area '" + areaId + "' has no mesh.";
                    return false;
                }
            }

            SkinnedMeshRenderer[] skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                if (skinnedRenderers[i] != null && skinnedRenderers[i].sharedMesh == null)
                {
                    error = "Skinned renderer '" + skinnedRenderers[i].name +
                            "' in area '" + areaId + "' has no mesh.";
                    return false;
                }
            }

            MeshCollider[] meshColliders = GetComponentsInChildren<MeshCollider>(true);
            for (int i = 0; i < meshColliders.Length; i++)
            {
                if (meshColliders[i] != null && meshColliders[i].enabled &&
                    meshColliders[i].sharedMesh == null)
                {
                    error = "A MeshCollider in streamed area '" + areaId + "' has no mesh.";
                    return false;
                }
            }

            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                error = "Streamed area '" + areaId + "' contains no renderers.";
                return false;
            }

            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                if (renderer == null) continue;

                Material[] materials = renderer.sharedMaterials;
                if (materials == null || materials.Length == 0)
                {
                    error = "Renderer '" + renderer.name + "' in area '" + areaId +
                            "' has no assigned materials.";
                    return false;
                }

                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    Material material = materials[materialIndex];
                    if (material == null || material.shader == null || !material.shader.isSupported)
                    {
                        error = "Renderer '" + renderer.name + "' in area '" + areaId +
                                "' has a missing or unsupported material shader reference.";
                        return false;
                    }
                }

                MeshFilter filter = renderer.GetComponent<MeshFilter>();
                if (filter != null && filter.sharedMesh != null &&
                    materials.Length < filter.sharedMesh.subMeshCount)
                {
                    error = "Renderer '" + renderer.name + "' in area '" + areaId +
                            "' has fewer materials than its mesh has submeshes.";
                    return false;
                }

                SkinnedMeshRenderer skinned = renderer as SkinnedMeshRenderer;
                if (skinned != null && skinned.sharedMesh != null &&
                    materials.Length < skinned.sharedMesh.subMeshCount)
                {
                    error = "Skinned renderer '" + renderer.name + "' in area '" + areaId +
                            "' has fewer materials than its mesh has submeshes.";
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }
    }
}
