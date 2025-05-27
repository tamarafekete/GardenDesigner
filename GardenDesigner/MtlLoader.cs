using System.Numerics;

public static class MtlLoader
{
    public static List<ObjMaterial> Load(string mtlPath)
    {
        var materials = new List<ObjMaterial>();
        ObjMaterial? current = null;

        foreach (var line in File.ReadLines(mtlPath))
        {
            string trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#")) continue;

            var tokens = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            switch (tokens[0])
            {
                case "newmtl":
                    current = new ObjMaterial { Name = tokens[1] };
                    materials.Add(current);
                    break;

                case "Kd":
                    if (current != null)
                    {
                        current.DiffuseColor = new Vector3(
                            float.Parse(tokens[1]),
                            float.Parse(tokens[2]),
                            float.Parse(tokens[3])
                        );
                    }
                    break;

                case "map_Kd":
                    if (current != null)
                        current.DiffuseTexturePath = tokens[1];
                    break;
            }
        }

        return materials;
    }
}

