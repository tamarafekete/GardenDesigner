using Silk.NET.OpenGL;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using Szeminarium1_24_02_17_2;

public class MaterialGroup
{
    public string MaterialName;
    public List<(int v, int vt, int vn)> Indices = new();
}

public class ObjMaterial
{
    public string Name { get; set; }
    public string DiffuseTexturePath { get; set; }
    public uint? TextureId { get; set; }

    // ADD THIS LINE
    public Vector3 DiffuseColor { get; set; } = new Vector3(1.0f, 1.0f, 1.0f);
}



public class ObjModel
{
    public List<Vector3> Positions { get; } = new();
    public List<Vector2> TexCoords { get; } = new();
    public List<Vector3> Normals { get; } = new();

    public Dictionary<string, ObjMaterial> Materials { get; } = new();

    // One group per material
    public List<MaterialGroup> MaterialGroups { get; } = new();

    // Each face is a tuple: (vertex index, texture index, normal index)
    public List<(int v, int vt, int vn)> Indices { get; } = new();

    public static ObjModel LoadFromFile(string objPath)
    {
        var model = new ObjModel();
        MaterialGroup? currentGroup = null;

        string? currentMaterial = null;

        foreach (var line in File.ReadLines(objPath))
        {
            string trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#")) continue;

            var tokens = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            switch (tokens[0])
            {
                case "v":
                    model.Positions.Add(ParseVec3(tokens));
                    break;

                case "vt":
                    model.TexCoords.Add(ParseVec2(tokens));
                    break;

                case "vn":
                    model.Normals.Add(ParseVec3(tokens));
                    break;

                case "usemtl":
                    currentMaterial = tokens[1];
                    currentGroup = new MaterialGroup { MaterialName = currentMaterial };
                    model.MaterialGroups.Add(currentGroup);
                    break;

                case "f":
                    if (currentGroup == null)
                    {
                        currentGroup = new MaterialGroup { MaterialName = "default" };
                        model.MaterialGroups.Add(currentGroup);
                    }

                    var vertices = tokens.Skip(1).Select(ParseVertex).ToList();
                    for (int i = 1; i < vertices.Count - 1; i++)
                    {
                        currentGroup.Indices.Add(vertices[0]);
                        currentGroup.Indices.Add(vertices[i]);
                        currentGroup.Indices.Add(vertices[i + 1]);
                    }
                    break;

                case "mtllib":
                    string mtlPath = Path.Combine(Path.GetDirectoryName(objPath)!, tokens[1]);
                    var materials = MtlLoader.Load(mtlPath);
                    foreach (var mat in materials)
                        model.Materials[mat.Name] = mat;
                    break;
            }
        }

        return model;
    }



    private static (int v, int vt, int vn) ParseVertex(string str)
    {
        var parts = str.Split('/');
        int v = int.Parse(parts[0]) - 1;
        int vt = parts.Length > 1 && parts[1] != "" ? int.Parse(parts[1]) - 1 : -1;
        int vn = parts.Length > 2 ? int.Parse(parts[2]) - 1 : -1;
        return (v, vt, vn);
    }

    private static Vector3 ParseVec3(string[] tokens) =>
        new(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]));

    private static Vector2 ParseVec2(string[] tokens) =>
        new(float.Parse(tokens[1]), float.Parse(tokens[2]));
}

