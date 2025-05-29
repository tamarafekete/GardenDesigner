using Silk.NET.OpenGL;
using System.Numerics;
using System.Xml;

namespace Szeminarium1_24_02_17_2
{
    internal static class ColladaReader
    {
        public static unsafe GlObject CreateObjectWithColor(GL gl, float[] color, string path)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("c", "http://www.collada.org/2005/11/COLLADASchema");

            var positionSource = xmlDoc.SelectSingleNode("//c:source[contains(@id,'positions')]/c:float_array", nsManager);
            string[] positionValues = positionSource.InnerText.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            float[] positions = Array.ConvertAll(positionValues, float.Parse);

            int vertexCount = positions.Length / 3;


            float[] colors = new float[vertexCount * 4];
            for (int i = 0; i < vertexCount; i++)
            {
                colors[i * 4 + 0] = color[0];
                colors[i * 4 + 1] = color[1];
                colors[i * 4 + 2] = color[2];
                colors[i * 4 + 3] = color[3];
            }

            var pNode = xmlDoc.SelectSingleNode("//c:triangles/c:p", nsManager);
            string[] indexStrings = pNode.InnerText.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int inputCount = xmlDoc.SelectNodes("//c:triangles/c:input", nsManager).Count;
            
            uint[] indices = new uint[indexStrings.Length / inputCount];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = uint.Parse(indexStrings[i * inputCount + 0]); 
            }

            var normalSource = xmlDoc.SelectSingleNode("//c:source[contains(@id,'-normals')]/c:float_array", nsManager);
            float[] normals;
            if (normalSource != null)
            {
                string[] normalValues = normalSource.InnerText.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                normals = Array.ConvertAll(normalValues, float.Parse);
            }
            else
            {
                normals = GenerateNormals(positions, indices);
            }

            
            uint vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);

            uint posBuffer = gl.GenBuffer();
            fixed (float* p = positions)
            {
                gl.BindBuffer(GLEnum.ArrayBuffer, posBuffer);
                gl.BufferData(GLEnum.ArrayBuffer, (nuint)(positions.Length * sizeof(float)), p, GLEnum.StaticDraw);
                gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 0, (void*)0);
                gl.EnableVertexAttribArray(0);
            }

            uint colBuffer = gl.GenBuffer();
            fixed (float* c = colors)
            {
                gl.BindBuffer(GLEnum.ArrayBuffer, colBuffer);
                gl.BufferData(GLEnum.ArrayBuffer, (nuint)(colors.Length * sizeof(float)), c, GLEnum.StaticDraw);
                gl.VertexAttribPointer(1, 4, GLEnum.Float, false, 0, (void*)0);
                gl.EnableVertexAttribArray(1);
            }

            uint normBuffer = gl.GenBuffer();
            fixed (float* n = normals)
            {
                gl.BindBuffer(GLEnum.ArrayBuffer, normBuffer);
                gl.BufferData(GLEnum.ArrayBuffer, (nuint)(normals.Length * sizeof(float)), n, GLEnum.StaticDraw);
                gl.VertexAttribPointer(2, 3, GLEnum.Float, false, 0, (void*)0);
                gl.EnableVertexAttribArray(2);
            }

            uint indexBuffer = gl.GenBuffer();
            fixed (uint* i = indices)
            {
                gl.BindBuffer(GLEnum.ElementArrayBuffer, indexBuffer);
                gl.BufferData(GLEnum.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), i, GLEnum.StaticDraw);
            }

            gl.BindVertexArray(0);

            return new GlObject(vao, posBuffer, colBuffer, indexBuffer, (uint)indices.Length, gl);
        }
    

        private static float[] GenerateNormals(float[] positions, uint[] indices)
        {
            int vertexCount = positions.Length / 3;
            var normals = new float[positions.Length];

            for (int i = 0; i < indices.Length; i += 3)
            {
                uint i0 = indices[i];
                uint i1 = indices[i + 1];
                uint i2 = indices[i + 2];

                var v0 = new System.Numerics.Vector3(positions[i0 * 3 + 0], positions[i0 * 3 + 1], positions[i0 * 3 + 2]);
                var v1 = new System.Numerics.Vector3(positions[i1 * 3 + 0], positions[i1 * 3 + 1], positions[i1 * 3 + 2]);
                var v2 = new System.Numerics.Vector3(positions[i2 * 3 + 0], positions[i2 * 3 + 1], positions[i2 * 3 + 2]);

                var edge1 = v1 - v0;
                var edge2 = v2 - v0;
                var normal = Vector3.Normalize(Vector3.Cross(edge1, edge2));

                for (int j = 0; j < 3; j++)
                {
                    uint idx = indices[i + j];
                    normals[idx * 3 + 0] += normal.X;
                    normals[idx * 3 + 1] += normal.Y;
                    normals[idx * 3 + 2] += normal.Z;
                }
            }

            for (int i = 0; i < vertexCount; i++)
            {
                var normal = new System.Numerics.Vector3(
                    normals[i * 3 + 0],
                    normals[i * 3 + 1],
                    normals[i * 3 + 2]
                );
                normal = Vector3.Normalize(normal);
                normals[i * 3 + 0] = normal.X;
                normals[i * 3 + 1] = normal.Y;
                normals[i * 3 + 2] = normal.Z;
            }

            return normals;
        }

    }
}

