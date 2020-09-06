using UnityEngine;
using UnityEngine.Rendering;

namespace Pedev {
    public class MyPipeline : RenderPipeline {
        readonly CameraRenderer cameraRenderer = new CameraRenderer();
        SortingCriteria opaqueSortingCriteria;
        SortingCriteria transparentSortingCriteria;
        public MyPipeline(SortingCriteria opaqueSortingCriteria, SortingCriteria transparentSortingCriteria) {
            this.opaqueSortingCriteria = opaqueSortingCriteria;
            this.transparentSortingCriteria = transparentSortingCriteria;
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
            foreach (var camera in cameras) {
                cameraRenderer.Render(context, camera, this.opaqueSortingCriteria, this.transparentSortingCriteria);
            }
        }
    }
}
