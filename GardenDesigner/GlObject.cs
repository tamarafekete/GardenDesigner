using Silk.NET.OpenGL;

namespace Szeminarium1_24_02_17_2
{
    internal class GlObject
    {
        public uint Vao { get; }
        public uint Vertices { get; }
        public uint Colors { get; }
        public uint Indices { get; }
        public uint IndexArrayLength { get; }
        public uint Texture { get; set; }

        private GL Gl;

        public GlObject(uint vao, uint vertices, uint colors, uint indices, uint indexArrayLength, GL gl)
        {
            Vao = vao;
            Vertices = vertices;
            Colors = colors;
            Indices = indices;
            IndexArrayLength = indexArrayLength;
            Gl = gl;
            Texture = 0;
        }

        internal void ReleaseGlObject()
        {
            Gl.DeleteBuffer(Vertices);
            Gl.DeleteBuffer(Colors);
            Gl.DeleteBuffer(Indices);
            Gl.DeleteVertexArray(Vao);
            if (Texture != 0)
            {
                Gl.DeleteTexture(Texture);
            }
        }
    }

}
