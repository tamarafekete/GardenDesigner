using Silk.NET.OpenGL;
using StbImageSharp;
using System;
using System.IO;

public static class TextureLoader
{
    public static unsafe uint LoadTexture(GL gl, string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        uint texture = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, texture);

        fixed (byte* dataPtr = image.Data)
        {
            gl.TexImage2D(
                TextureTarget.Texture2D,   // target
                0,                         // level
                (int)InternalFormat.Rgba,  // internal format
                (uint)image.Width,         // width
                (uint)image.Height,        // height
                0,                         // border
                PixelFormat.Rgba,          // format
                PixelType.UnsignedByte,    // type
                dataPtr                    // pointer to pixel data
            );
        }

        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);

        gl.GenerateMipmap(TextureTarget.Texture2D);

        return texture;
    }


}
