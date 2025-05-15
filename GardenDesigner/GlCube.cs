﻿using Silk.NET.OpenGL;
using StbImageSharp;

namespace Szeminarium1_24_02_17_2
{
    internal class GlCube : GlObject
    {
        public uint? Texture { get; private set; }

        private GlCube(uint vao, uint vertices, uint colors, uint indeces, uint indexArrayLength, GL gl, uint texture = 0)
            : base(vao, vertices, colors, indeces, indexArrayLength, gl)
        {
            Texture = texture;
        }

        public static unsafe GlCube CreateCubeWithFaceColors(GL Gl, float[] face1Color, float[] face2Color, float[] face3Color, float[] face4Color, float[] face5Color, float[] face6Color)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            // counter clockwise is front facing
            float[] vertexArray = new float[] {
                // top face
                -0.5f, 0.5f, 0.5f, 0f, 1f, 0f,
                0.5f, 0.5f, 0.5f, 0f, 1f, 0f,
                0.5f, 0.5f, -0.5f, 0f, 1f, 0f,
                -0.5f, 0.5f, -0.5f, 0f, 1f, 0f,

                // front face
                -0.5f, 0.5f, 0.5f, 0f, 0f, 1f,
                -0.5f, -0.5f, 0.5f, 0f, 0f, 1f,
                0.5f, -0.5f, 0.5f, 0f, 0f, 1f,
                0.5f, 0.5f, 0.5f, 0f, 0f, 1f,

                // left face
                -0.5f, 0.5f, 0.5f, -1f, 0f, 0f,
                -0.5f, 0.5f, -0.5f, -1f, 0f, 0f,
                -0.5f, -0.5f, -0.5f, -1f, 0f, 0f,
                -0.5f, -0.5f, 0.5f, -1f, 0f, 0f,

                // bottom face
                -0.5f, -0.5f, 0.5f, 0f, -1f, 0f,
                0.5f, -0.5f, 0.5f,0f, -1f, 0f,
                0.5f, -0.5f, -0.5f,0f, -1f, 0f,
                -0.5f, -0.5f, -0.5f,0f, -1f, 0f,

                // back face
                0.5f, 0.5f, -0.5f, 0f, 0f, -1f,
                -0.5f, 0.5f, -0.5f,0f, 0f, -1f,
                -0.5f, -0.5f, -0.5f,0f, 0f, -1f,
                0.5f, -0.5f, -0.5f,0f, 0f, -1f,

                // right face
                0.5f, 0.5f, 0.5f, 1f, 0f, 0f,
                0.5f, 0.5f, -0.5f,1f, 0f, 0f,
                0.5f, -0.5f, -0.5f,1f, 0f, 0f,
                0.5f, -0.5f, 0.5f,1f, 0f, 0f
            };

            List<float> colorsList = new List<float>();
            colorsList.AddRange(face1Color);
            colorsList.AddRange(face1Color);
            colorsList.AddRange(face1Color);
            colorsList.AddRange(face1Color);

            colorsList.AddRange(face2Color);
            colorsList.AddRange(face2Color);
            colorsList.AddRange(face2Color);
            colorsList.AddRange(face2Color);

            colorsList.AddRange(face3Color);
            colorsList.AddRange(face3Color);
            colorsList.AddRange(face3Color);
            colorsList.AddRange(face3Color);

            colorsList.AddRange(face4Color);
            colorsList.AddRange(face4Color);
            colorsList.AddRange(face4Color);
            colorsList.AddRange(face4Color);

            colorsList.AddRange(face5Color);
            colorsList.AddRange(face5Color);
            colorsList.AddRange(face5Color);
            colorsList.AddRange(face5Color);

            colorsList.AddRange(face6Color);
            colorsList.AddRange(face6Color);
            colorsList.AddRange(face6Color);
            colorsList.AddRange(face6Color);


            float[] colorArray = colorsList.ToArray();

            uint[] indexArray = new uint[] {
                0, 1, 2,
                0, 2, 3,

                4, 5, 6,
                4, 6, 7,

                8, 9, 10,
                10, 11, 8,

                12, 14, 13,
                12, 15, 14,

                17, 16, 19,
                17, 19, 18,

                20, 22, 21,
                20, 23, 22
            };

            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint vertexSize = offsetNormal + (3 * sizeof(float));

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);

            // release array buffer
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            uint indexArrayLength = (uint)indexArray.Length;

            return new GlCube(vao, vertices, colors, indices, indexArrayLength, Gl);
        }

        public static unsafe GlCube CreateSquare(GL Gl, float[] faceColor)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            // Vertex data: position (3), normal (3), texcoord (2) = 8 floats per vertex
            float repeat = 10f;
            float[] vertexArray = new float[]
            {
                //    Position           Normal         UV
                -20f, 0f,  20f,     0f, 1f, 0f,     0f,      0f,
                 20f, 0f,  20f,     0f, 1f, 0f,     repeat,  0f,
                 20f, 0f, -20f,     0f, 1f, 0f,     repeat,  repeat,
                -20f, 0f, -20f,     0f, 1f, 0f,     0f,      repeat
            };


            uint[] indexArray = new uint[]
            {
        0, 1, 2,
        0, 2, 3
            };

            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint offsetTexCoord = offsetNormal + (3 * sizeof(float));
            uint vertexSize = offsetTexCoord + (2 * sizeof(float));

            // Vertex buffer
            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertices);
            fixed (float* v = vertexArray)
                Gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexArray.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);

            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);  // position
            Gl.EnableVertexAttribArray(0);

            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);  // normal
            Gl.EnableVertexAttribArray(2);

            Gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetTexCoord);  // UV
            Gl.EnableVertexAttribArray(3);

            // Dummy color buffer (not used in lighting-based shaders)
            float[] colorArray = new float[4 * 4];
            for (int i = 0; i < 4; i++) Array.Copy(faceColor, 0, colorArray, i * 4, 4);

            uint colorBuffer = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, colorBuffer);
            fixed (float* c = colorArray)
                Gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(colorArray.Length * sizeof(float)), c, BufferUsageARB.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, (void*)0);
            Gl.EnableVertexAttribArray(1);

            // Index buffer
            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, indices);
            fixed (uint* i = indexArray)
                Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indexArray.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);

            Gl.BindVertexArray(0);

            return new GlCube(vao, vertices, colorBuffer, indices, (uint)indexArray.Length, Gl);
        }

        public static unsafe GlCube CreateInteriorCube(GL Gl, string textureResourceId)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            // counter clockwise is front facing
            float[] vertexArray = new float[] {
                // top face
                -0.5f, 0.5f, 0.5f, 0f, -1f, 0f, 1f/4f, 0f/3f,
                0.5f, 0.5f, 0.5f, 0f, -1f, 0f, 2f/4f, 0f/3f,
                0.5f, 0.5f, -0.5f, 0f, -1f, 0f, 2f/4f, 1f/3f,
                -0.5f, 0.5f, -0.5f, 0f, -1f, 0f, 1f/4f, 1f/3f,

                // front face
                -0.5f, 0.5f, 0.5f, 0f, 0f, -1f, 1, 1f/3f,
                -0.5f, -0.5f, 0.5f, 0f, 0f, -1f, 4f/4f, 2f/3f,
                0.5f, -0.5f, 0.5f, 0f, 0f, -1f, 3f/4f, 2f/3f,
                0.5f, 0.5f, 0.5f, 0f, 0f, -1f,  3f/4f, 1f/3f,

                // left face
                -0.5f, 0.5f, 0.5f, 1f, 0f, 0f, 0, 1f/3f,
                -0.5f, 0.5f, -0.5f, 1f, 0f, 0f,1f/4f, 1f/3f,
                -0.5f, -0.5f, -0.5f, 1f, 0f, 0f, 1f/4f, 2f/3f,
                -0.5f, -0.5f, 0.5f, 1f, 0f, 0f, 0f/4f, 2f/3f,

                // bottom face
                -0.5f, -0.5f, 0.5f, 0f, 1f, 0f, 1f/4f, 1f,
                0.5f, -0.5f, 0.5f,0f, 1f, 0f, 2f/4f, 1f,
                0.5f, -0.5f, -0.5f,0f, 1f, 0f, 2f/4f, 2f/3f,
                -0.5f, -0.5f, -0.5f,0f, 1f, 0f, 1f/4f, 2f/3f,

                // back face
                0.5f, 0.5f, -0.5f, 0f, 0f, 1f, 2f/4f, 1f/3f,
                -0.5f, 0.5f, -0.5f, 0f, 0f, 1f, 1f/4f, 1f/3f,
                -0.5f, -0.5f, -0.5f,0f, 0f, 1f, 1f/4f, 2f/3f,
                0.5f, -0.5f, -0.5f,0f, 0f, 1f, 2f/4f, 2f/3f,

                // right face
                0.5f, 0.5f, 0.5f, -1f, 0f, 0f, 3f/4f, 1f/3f,
                0.5f, 0.5f, -0.5f,-1f, 0f, 0f, 2f/4f, 1f/3f,
                0.5f, -0.5f, -0.5f, -1f, 0f, 0f, 2f/4f, 2f/3f,
                0.5f, -0.5f, 0.5f, -1f, 0f, 0f, 3f/4f, 2f/3f,
            };

            uint[] indexArray = new uint[] {
                0, 2, 1,
                0, 3, 2,

                4, 6, 5,
                4, 7, 6,

                8, 10, 9,
                10, 8, 11,

                12, 13, 14,
                12, 14, 15,

                17, 19, 16,
                17, 18, 19,

                20, 21, 22,
                20, 22, 23
            };

            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint offsetTexture = offsetNormal + (3 * sizeof(float));
            uint vertexSize = offsetTexture + (2 * sizeof(float));

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);

            uint colors = Gl.GenBuffer();
            //Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            //Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            //Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            //Gl.EnableVertexAttribArray(1);

            // set texture
            // create texture
            uint texture = Gl.GenTexture();
            // activate texture 0
            Gl.ActiveTexture(TextureUnit.Texture0);
            // bind texture
            Gl.BindTexture(TextureTarget.Texture2D, texture);

            var skyboxImageResult = ReadTextureImage("skybox.png");
            var textureBytes = (ReadOnlySpan<byte>)skyboxImageResult.Data.AsSpan();
            // Here we use "result.Width" and "result.Height" to tell OpenGL about how big our texture is.
            Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)skyboxImageResult.Width,
                (uint)skyboxImageResult.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, textureBytes);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            // unbinde texture
            Gl.BindTexture(TextureTarget.Texture2D, 0);

            Gl.EnableVertexAttribArray(3);
            Gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetTexture);


            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);

            // release array buffer
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            uint indexArrayLength = (uint)indexArray.Length;

            return new GlCube(vao, vertices, colors, indices, indexArrayLength, Gl, texture);
        }


        private static unsafe ImageResult ReadTextureImage(string textureResource)
        {
            ImageResult result;
            using (Stream skyeboxStream
                = typeof(GlCube).Assembly.GetManifestResourceStream("GardenDesigner.Resources." + textureResource))
                result = ImageResult.FromStream(skyeboxStream, ColorComponents.RedGreenBlueAlpha);

            return result;
        }
    }
}
