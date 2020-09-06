using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pedev {
    [RequireComponent(typeof(Renderer))]
    public class SpriteRenderOrder : MonoBehaviour {
        [Range(0, 31)]
        public int sortingLayerID = 0;
        [Range(-20, 20)]
        public int orderInLayer = 0;
        [Range(-20, 20)]
        public int rendererPriority = 0;

        private Renderer m_Renderer;
        private Renderer _Renderer {
            get {
                if (!m_Renderer) {
                    m_Renderer = this.GetComponent<Renderer>();
                }
                return m_Renderer;
            }
        }

        #region Unity Methods
        // Initialize my components.
        void Awake() {
            Validate();
        }

        private void OnValidate() {
            Validate();
        }

        private void Validate() {
            _Renderer.sortingLayerID = SortingLayer.layers[this.sortingLayerID].id;
            _Renderer.sortingOrder = this.orderInLayer;
            _Renderer.rendererPriority = this.rendererPriority;
        }
        #endregion

    }
}