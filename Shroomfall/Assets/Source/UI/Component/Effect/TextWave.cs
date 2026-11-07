using TMPro;
using UnityEngine;

namespace Assets.Source.UI.Component.Effect
{
    public class TextWave : MonoBehaviour
    {
        #region Attributes
        [Header("Animation")]
        [SerializeField] private float amplitude = 5f;
        [SerializeField] private float frequency = 2f;
        [SerializeField] private float speed = 4f;
        [SerializeField] private TMP_Text textComponent;

        private TMP_MeshInfo[] cachedMeshInfo;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void OnEnable()
        {
            textComponent.ForceMeshUpdate();
            cachedMeshInfo = textComponent.textInfo.CopyMeshInfoVertexData();
        }

        private void LateUpdate()
        {
            if (textComponent == null || !textComponent.enabled)
                return;

            textComponent.ForceMeshUpdate();
            TMP_TextInfo textInfo = textComponent.textInfo;
            if (cachedMeshInfo == null || cachedMeshInfo.Length != textInfo.meshInfo.Length)
                cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible)
                    continue;

                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;
                if (materialIndex >= cachedMeshInfo.Length || materialIndex >= textInfo.meshInfo.Length)
                    continue;

                Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;
                Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                if (vertexIndex + 3 >= sourceVertices.Length || vertexIndex + 3 >= destinationVertices.Length)
                    continue;

                Vector3 offset = new Vector3(0f, Mathf.Sin(Time.time * speed - i * frequency) * amplitude, 0f);

                for (int j = 0; j < 4; j++)
                    destinationVertices[vertexIndex + j] = sourceVertices[vertexIndex + j] + offset;
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                if (textInfo.meshInfo[i].mesh == null)
                    continue;

                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }
        #endregion
    }
}