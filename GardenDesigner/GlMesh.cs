using Silk.NET.OpenGL;
using System.Numerics;

public class GLMesh
{
    public List<GLSubMesh> SubMeshes { get; } = new();

    public GLMesh(GL gl, ObjModel model)
    {
        foreach (var group in model.MaterialGroups)
        {
            SubMeshes.Add(new GLSubMesh(gl, group, model));
        }
    }

    public unsafe void Draw(GL gl, ObjModel model, uint shaderProgram)
    {
        foreach (var mesh in SubMeshes)
        {
            if (model.Materials.TryGetValue(mesh.MaterialName, out var material) && material.TextureId.HasValue)
            {
                gl.ActiveTexture(TextureUnit.Texture0);
                gl.BindTexture(TextureTarget.Texture2D, material.TextureId.Value);

                int texLoc = gl.GetUniformLocation(shaderProgram, "uTexture");
                gl.Uniform1(texLoc, 0);
            }

            gl.BindVertexArray(mesh.VAO);
            gl.DrawElements(PrimitiveType.Triangles, (uint)mesh.IndexCount, DrawElementsType.UnsignedInt, null);
        }

        gl.BindVertexArray(0);
    }
}

public class GLSubMesh
{
    public uint VAO, VBO, EBO;
    public int IndexCount;
    public string MaterialName;

    public GLSubMesh(GL gl, MaterialGroup group, ObjModel model)
    {
        MaterialName = group.MaterialName;

        var vertexData = new List<float>();
        var indexData = new List<uint>();
        var vertexMap = new Dictionary<(int v, int vt, int vn), uint>();
        uint index = 0;

        foreach (var (v, vt, vn) in group.Indices)
        {
            var key = (v, vt, vn);
            if (!vertexMap.TryGetValue(key, out uint idx))
            {
                var pos = model.Positions[v];
                var tex = vt >= 0 ? model.TexCoords[vt] : Vector2.Zero;
                var norm = vn >= 0 ? model.Normals[vn] : Vector3.UnitY;

                vertexData.AddRange(new float[] {
                    pos.X, pos.Y, pos.Z,
                    norm.X, norm.Y, norm.Z,
                    tex.X, tex.Y
                });

                idx = index++;
                vertexMap[key] = idx;
            }
            indexData.Add(idx);
        }

        IndexCount = indexData.Count;

        VAO = gl.GenVertexArray();
        VBO = gl.GenBuffer();
        EBO = gl.GenBuffer();

        gl.BindVertexArray(VAO);

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);
        float[] vertexArray = vertexData.ToArray();
        gl.BufferData<float>(BufferTargetARB.ArrayBuffer, vertexData.ToArray(), BufferUsageARB.StaticDraw);

        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, EBO);
        gl.BufferData<uint>(BufferTargetARB.ElementArrayBuffer, indexData.ToArray(), BufferUsageARB.StaticDraw);

        int stride = sizeof(float) * 8;
        gl.EnableVertexAttribArray(0); // position
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)stride, 0);
        gl.EnableVertexAttribArray(1); // normal
        gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (uint)stride, 12);
        gl.EnableVertexAttribArray(2); // texcoord
        gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, (uint)stride, 24);

        gl.BindVertexArray(0);
    }
}

