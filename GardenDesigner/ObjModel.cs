using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;

public class ObjModel
{
    public List<Vector3> Vertices { get; } = new();
    public List<Vector3> Normals { get; } = new();
    public List<Vector2> TexCoords { get; } = new();

    // Each face is a tuple: (vertex index, texture index, normal index)
    public List<(int v, int vt, int vn)> Indices { get; } = new();

    public static ObjModel LoadFromFile(string path)
    {
        using var stream = File.OpenRead(path);
        return LoadFromStream(stream);
    }

    public static ObjModel LoadFromStream(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var model = new ObjModel();
        var culture = CultureInfo.InvariantCulture;

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

            var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) continue;

            switch (tokens[0])
            {
                case "v":
                    model.Vertices.Add(ParseVec3(tokens, culture));
                    break;
                case "vt":
                    model.TexCoords.Add(ParseVec2(tokens, culture));
                    break;
                case "vn":
                    model.Normals.Add(ParseVec3(tokens, culture));
                    break;
                case "f":
                    {
                        // Parse all vertex triplets for the face
                        var faceVertices = new List<(int v, int vt, int vn)>();

                        for (int i = 1; i < tokens.Length; i++)
                        {
                            var parts = tokens[i].Split('/');
                            int v = int.Parse(parts[0]) - 1;
                            int vt = parts.Length > 1 && parts[1] != "" ? int.Parse(parts[1]) - 1 : -1;
                            int vn = parts.Length > 2 ? int.Parse(parts[2]) - 1 : -1;
                            faceVertices.Add((v, vt, vn));
                        }

                        // Triangulate polygon with fan method
                        for (int i = 1; i < faceVertices.Count - 1; i++)
                        {
                            model.Indices.Add(faceVertices[0]);
                            model.Indices.Add(faceVertices[i]);
                            model.Indices.Add(faceVertices[i + 1]);
                        }
                        break;
                    }

            }
        }

        return model;
    }

    private static Vector3 ParseVec3(string[] tokens, CultureInfo culture)
    {
        return new Vector3(
            float.Parse(tokens[1], culture),
            float.Parse(tokens[2], culture),
            float.Parse(tokens[3], culture));
    }

    private static Vector2 ParseVec2(string[] tokens, CultureInfo culture)
    {
        return new Vector2(
            float.Parse(tokens[1], culture),
            float.Parse(tokens[2], culture));
    }
}

