using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pedev {
    [CreateAssetMenu]
    public class MyPipelineAsset : RenderPipelineAsset {
        public SortingCriteria opaqueSortingCriteria = SortingCriteria.CommonOpaque;
        public SortingCriteria transparentSortingCriteria = SortingCriteria.CommonTransparent;
        protected override RenderPipeline CreatePipeline() {
            return new MyPipeline(opaqueSortingCriteria, transparentSortingCriteria);
        }
    }
}
