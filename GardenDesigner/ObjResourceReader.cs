using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System.Globalization;

namespace Szeminarium1_24_02_17_2
{
    internal class ObjResourceReader
    {
        /*public static unsafe GlObject CreateTeapotWithColor(GL Gl, float[] faceColor)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            List<float[]> objVertices;
            List<int[]> objFaces;

            ReadObjDataForTeapot(out objVertices, out objFaces);

            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            List<uint> glIndices = new List<uint>();

            CreateGlArraysFromObjArrays(faceColor, objVertices, objFaces, glVertices, glColors, glIndices);

            return CreateOpenGlObject(Gl, vao, glVertices, glColors, glIndices);
        }*/

        public static unsafe GlObject CreateObjectWithColor(GL Gl, float[] faceColor, string resource)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            List<float[]> objVertices;
            List<int[]> objFaces;

            ReadObjDataForNewObject(out objVertices, out objFaces, out var objNormals, out var objNormalIndices, resource);

            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            List<uint> glIndices = new List<uint>();

            CreateGlArraysFromObjArrays(faceColor, objVertices, objFaces, objNormals, objNormalIndices, glVertices, glColors, glIndices);

            return CreateOpenGlObject(Gl, vao, glVertices, glColors, glIndices);
        }



        private static unsafe GlObject CreateOpenGlObject(GL Gl, uint vao, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint vertexSize = offsetNormal + (3 * sizeof(float));

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glVertices.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glColors.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)glIndices.ToArray().AsSpan(), GLEnum.StaticDraw);

            // release array buffer
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            uint indexArrayLength = (uint)glIndices.Count;

            return new GlObject(vao, vertices, colors, indices, indexArrayLength, Gl);
        }

        private static unsafe void CreateGlArraysFromObjArrays(float[] faceColor, List<float[]> objVertices, List<int[]> objFaces, List<float[]>objNormals, List<int[]>objNormalIndices, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            Dictionary<string, int> glVertexIndices = new Dictionary<string, int>();

            for (int f = 0; f < objFaces.Count; f++)
            {
                var face = objFaces[f];
                var normalIdx = objNormalIndices[f];

                Vector3D<float> fallbackNormal = default;

                if (normalIdx[0] <= 0)
                {
                    // Compute normal only if it's not provided
                    var aVertex = objVertices[face[0] - 1];
                    var bVertex = objVertices[face[1] - 1];
                    var cVertex = objVertices[face[2] - 1];

                    var a = new Vector3D<float>(aVertex[0], aVertex[1], aVertex[2]);
                    var b = new Vector3D<float>(bVertex[0], bVertex[1], bVertex[2]);
                    var c = new Vector3D<float>(cVertex[0], cVertex[1], cVertex[2]);

                    fallbackNormal = Vector3D.Normalize(Vector3D.Cross(b - a, c - a));
                }

                for (int i = 0; i < 3; i++)
                {
                    var vertex = objVertices[face[i] - 1];
                    Vector3D<float> normal;

                    if (normalIdx[i] > 0)
                    {
                        var n = objNormals[normalIdx[i] - 1];
                        normal = new Vector3D<float>(n[0], n[1], n[2]);
                    }
                    else
                    {
                        normal = fallbackNormal;
                    }

                    List<float> glVertex = new();
                    glVertex.AddRange(vertex);
                    glVertex.Add(normal.X);
                    glVertex.Add(normal.Y);
                    glVertex.Add(normal.Z);

                    var key = string.Join(" ", glVertex);
                    if (!glVertexIndices.ContainsKey(key))
                    {
                        glVertices.AddRange(glVertex);
                        glColors.AddRange(faceColor);
                        glVertexIndices[key] = glVertexIndices.Count;
                    }

                    glIndices.Add((uint)glVertexIndices[key]);
                }
            }
        }

        private static unsafe void ReadObjDataForTeapot(out List<float[]> objVertices, out List<int[]> objFaces)
        {
            objVertices = new List<float[]>();
            objFaces = new List<int[]>();
            using (Stream objStream = typeof(ObjResourceReader).Assembly.GetManifestResourceStream("Lab4_1.Resources.teapot.obj"))
            using (StreamReader objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine();

                    if (String.IsNullOrEmpty(line) || line.Trim().StartsWith("#"))
                        continue;

                    var lineClassifier = line.Substring(0, line.IndexOf(' '));
                    var lineData = line.Substring(lineClassifier.Length).Trim().Split(' ');

                    switch (lineClassifier)
                    {
                        case "v":
                            float[] vertex = new float[3];
                            for (int i = 0; i < vertex.Length; ++i)
                                vertex[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            objVertices.Add(vertex);
                            break;
                        case "f":
                            int[] face = new int[3];
                            for (int i = 0; i < face.Length; ++i)
                                face[i] = int.Parse(lineData[i]);
                            objFaces.Add(face);
                            break;
                    }
                }
            }
        }

        private static unsafe void ReadObjDataForNewObject(out List<float[]> objVertices, out List<int[]> objFaces, out List<float[]> objNormals, out List<int[]> objectNormalIndices, String resource)
        {
            objVertices = new List<float[]>();
            objFaces = new List<int[]>();
            objNormals = new List<float[]>();
            objectNormalIndices = new List<int[]>();

            using (Stream objStream = typeof(ObjResourceReader).Assembly.GetManifestResourceStream(resource))
            using (StreamReader objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine();

                    if (String.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#"))
                        continue;

                    var tokens = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var type = tokens[0];
                    var data = tokens[1..];

                    switch (type)
                    {
                        case "v":
                            if (data.Length < 3)
                                continue; // Skip malformed vertex lines

                            float[] vertex = new float[3];
                            for (int i = 0; i < 3; i++)
                                vertex[i] = float.Parse(data[i], CultureInfo.InvariantCulture);
                            objVertices.Add(vertex);
                            break;

                        case "vn":
                            float[] normal = new float[3];
                            for (int i = 0; i < 3; i++)
                                normal[i] = float.Parse(data[i], CultureInfo.InvariantCulture);
                            objNormals.Add(normal);
                            break;

                        case "f":
                            int[] faceIndices = new int[3];
                            int[] normalIndices = new int[3];

                            for (int i = 0; i < 3; i++)
                            {
                                var parts = data[i].Split('/');
                                faceIndices[i] = int.Parse(parts[0]);

                                if (parts.Length == 3 && !string.IsNullOrEmpty(parts[2]))
                                    normalIndices[i] = int.Parse(parts[2]);
                                else
                                    normalIndices[i] = -1;
                            }

                            objFaces.Add(faceIndices);
                            objectNormalIndices.Add(normalIndices);
                            break;
                    }

                }
            }
        }
    }
}
