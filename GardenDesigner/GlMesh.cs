
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System.Numerics;
using System;
using System.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
public class GLMesh
{
    public uint VAO;
    public uint VBO;
    public uint EBO;
    public int IndexCount;

    public unsafe GLMesh(GL gl, ObjModel model)
    {
        IndexCount = model.Indices.Count;

        float[] vertexData = new float[model.Indices.Count * 8]; // pos(3) + uv(2) + normal(3)
        uint[] indexData = new uint[model.Indices.Count];

        for (int i = 0; i < model.Indices.Count; i++)
        {
            var (vIdx, vtIdx, vnIdx) = model.Indices[i];
            var pos = model.Vertices[vIdx];
            var uv = vtIdx >= 0 ? model.TexCoords[vtIdx] : Vector2.Zero;
            var normal = vnIdx >= 0 ? model.Normals[vnIdx] : Vector3.UnitY;

            vertexData[i * 8 + 0] = pos.X;
            vertexData[i * 8 + 1] = pos.Y;
            vertexData[i * 8 + 2] = pos.Z;
            vertexData[i * 8 + 3] = uv.X;
            vertexData[i * 8 + 4] = uv.Y;
            vertexData[i * 8 + 5] = normal.X;
            vertexData[i * 8 + 6] = normal.Y;
            vertexData[i * 8 + 7] = normal.Z;

            indexData[i] = (uint)i; // each vertex is unique after interleaving
        }

        // Create VAO, VBO, EBO
        VAO = gl.GenVertexArray();
        VBO = gl.GenBuffer();
        EBO = gl.GenBuffer();

        gl.BindVertexArray(VAO);

        gl.BindBuffer(GLEnum.ArrayBuffer, VBO);
        fixed (float* v = &vertexData[0])
        {
            gl.BufferData(GLEnum.ArrayBuffer, (nuint)(vertexData.Length * sizeof(float)), v, GLEnum.StaticDraw);
        }

        gl.BindBuffer(GLEnum.ElementArrayBuffer, EBO);
        fixed (uint* i = &indexData[0])
        {
            gl.BufferData(GLEnum.ElementArrayBuffer, (nuint)(indexData.Length * sizeof(uint)), i, GLEnum.StaticDraw);
        }

        // Position
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 8 * sizeof(float), (void*)0);
        // TexCoord
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 2, GLEnum.Float, false, 8 * sizeof(float), (void*)(3 * sizeof(float)));
        // Normal
        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(2, 3, GLEnum.Float, false, 8 * sizeof(float), (void*)(5 * sizeof(float)));

        gl.BindVertexArray(0);
    }

    public unsafe void Draw(GL gl)
    {
        gl.BindVertexArray(VAO);
        gl.DrawElements(GLEnum.Triangles, (uint)IndexCount, GLEnum.UnsignedInt, null);
        gl.BindVertexArray(0);
    }
}
