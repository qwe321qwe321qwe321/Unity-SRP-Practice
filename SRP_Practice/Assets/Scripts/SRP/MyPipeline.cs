using UnityEngine;
using UnityEngine.Rendering;

namespace Pedev {
    public class MyPipeline : RenderPipeline {
        readonly CameraRenderer cameraRenderer = new CameraRenderer();
        protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
            foreach (var camera in cameras) {
                cameraRenderer.Render(context, camera);
            }
        }
    }
}
