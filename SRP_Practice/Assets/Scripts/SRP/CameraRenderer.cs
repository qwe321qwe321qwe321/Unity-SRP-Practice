using UnityEngine;
using UnityEngine.Rendering;

namespace Pedev {
    internal partial class CameraRenderer {
        ScriptableRenderContext context;
        Camera camera;
        const string CameraBufferName = "Render Camera";
        const string OpaqueBufferName = "Opaque";
        const string TransparentBufferName = "Transparent";
        CommandBuffer cameraBuffer = new CommandBuffer(){
            name = CameraBufferName
        };
        // Beacuse of nest sampling need different buffers. 
        // refer: https://forum.unity.com/threads/profilingsample-usage-in-custom-srp.638941/
        CommandBuffer opaqueBuffer = new CommandBuffer(){
            name = OpaqueBufferName
        };
        CommandBuffer transparentBuffer = new CommandBuffer(){
            name = TransparentBufferName
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
            BeginSampleBuffer(opaqueBuffer);
            var sortingSettings = new SortingSettings(camera){
                criteria = SortingCriteria.CommonOpaque
            };
            var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            context.DrawRenderers(
                cullingResults, ref drawingSettings, ref filteringSettings
            );
            EndSampleBuffer(opaqueBuffer);

            // Draw skybox.
            context.DrawSkybox(camera);

            // Draw transparent.
            BeginSampleBuffer(transparentBuffer);
            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
            EndSampleBuffer(transparentBuffer);
        }



        void Setup() {
            // mul unity_MatrixVP 
            this.context.SetupCameraProperties(camera);
            // Clear RT by camera parameters.
            CameraClearFlags clearFlags = camera.clearFlags;
            cameraBuffer.ClearRenderTarget(
                clearFlags <= CameraClearFlags.Depth, // 除了Flags.Nothing以外都會需要清除Depth
                clearFlags == CameraClearFlags.Color, // 只有Flags.Color需要清除，因為其他的情形要不就DepthOnly, Nothing要不然就Skybox(之後會直接蓋掉)
                clearFlags == CameraClearFlags.Color ? // 有清Color才需要給background color.
                    camera.backgroundColor.linear : Color.clear
            );

            cameraBuffer.BeginSample(cameraBuffer.name);
            ExecuteBuffer(cameraBuffer); // 執行Clear + BeginSample
        }

        void Submit() {
            EndSampleBuffer(cameraBuffer); // 執行EndSample
            this.context.Submit();
        }

        void BeginSampleBuffer(CommandBuffer buffer) {
            buffer.BeginSample(buffer.name);
            ExecuteBuffer(buffer);
        }
        void EndSampleBuffer(CommandBuffer buffer) {
            buffer.EndSample(buffer.name);
            ExecuteBuffer(buffer);
        }

        void ExecuteBuffer(CommandBuffer buffer) {
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