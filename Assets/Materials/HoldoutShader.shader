Shader "Custom/HoldoutShader"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" ="Geometry-1" "RenderPipeLine"="UniversalPipeLine"}
        Pass
        {
            // Do not write color to the framebuffer
            ColorMask 0
            
            // We need double sided since it is on GLB
            Cull Off

            // Use the stencil buffer to mark pixels
            Stencil
            {
                Ref 1
                Comp always
                Pass replace
            }
        }
    }
}
