using UnityEngine;
using UnityEngine.Rendering;

namespace Pedev {
    internal partial class CameraRenderer {
        ScriptableRenderContext context;
        Camera camera;
        const string BufferName = "Render Camera";
        CommandBuffer buffer = new CommandBuffer(){
            name = BufferName
        };
        CullingResults cullingResults;

        public void Render(ScriptableRenderContext context, Camera camera) {
            this.context = context;
            this.camera = camera;
            PrepareBuffer();
            PrepareUIGeometryForSceneWindow();
            if (!Cull()) {
                return;
            }

            Setup();
            DrawVisibleGeometry();
            DrawUnsupportedShaders();
            DrawGizmos();
            Submit();
        }

        static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
        void DrawVisibleGeometry() {
            // Draw opaque.
            var sortingSettings = new SortingSettings(camera){
                criteria = SortingCriteria.CommonOpaque
            };
            var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            context.DrawRenderers(
                cullingResults, ref drawingSettings, ref filteringSettings
            );

            // Draw skybox.
            context.DrawSkybox(camera);

            // Draw transparent.
            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        }

        

        void Setup() {
            // mul unity_MatrixVP 
            this.context.SetupCameraProperties(camera);
            // Clear RT by camera parameters.
            CameraClearFlags clearFlags = camera.clearFlags;
            buffer.ClearRenderTarget(
                clearFlags <= CameraClearFlags.Depth, // 除了Flags.Nothing以外都會需要清除Depth
                clearFlags == CameraClearFlags.Color, // 只有Flags.Color需要清除，因為其他的情形要不就DepthOnly, Nothing要不然就Skybox(之後會直接蓋掉)
                clearFlags == CameraClearFlags.Color ? // 有清Color才需要給background color.
                    camera.backgroundColor.linear : Color.clear
            );

            buffer.BeginSample(SampleName);
            ExecuteBuffer(); // 執行Clear + BeginSample
        }

        void Submit() {
            buffer.EndSample(SampleName);
            ExecuteBuffer(); // 執行EndSample
            this.context.Submit();
        }

        void ExecuteBuffer() {
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        /// <summary>
        /// 對Context根據Camera參數來做Culling，並將結果存至cullingResults
        /// </summary>
        /// <returns></returns>
        bool Cull() {
            if (camera.TryGetCullingParameters(out ScriptableCullingParameters p)) {
                cullingResults = context.Cull(ref p);
                return true;
            }
            return false;
        }
    }
}