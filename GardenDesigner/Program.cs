using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;


namespace Szeminarium1_24_02_17_2
{
    internal static class Program
    {
        private static CameraDescriptor cameraDescriptor;
        private static ExternalCameraDescriptor externalCameraDescriptor;
        private static bool externalCamera = false;
        private static GardenerArrangementModel gardenerArrangementModel = new();

        private static bool[,] positionMatrix;

        private static IWindow window;

        private static IInputContext inputContext;

        private static GL Gl;

        private static ImGuiController controller;

        private static uint program;

        private static GlObject ground;

        private static GlCube skyBox;

        private static ObjModel gardener;
        private static GLMesh gardenerMesh;

        private static ObjModel fence;
        private static GLMesh fenceMesh;

        private static ObjModel[] trees;
        private static float[] treeScales;

        private static int[] plantIndexes;
        private static GLMesh[] plantMeshes;
        private static int plantLength = 2;
        private static Tuple<int, int>[] plantCoords;
        private static string[] plantNames;
        private static int selectedIndex = 0;

        private static GlObject cat1, cat2;
        private static CatArrangementModel catArrangementModel = new(), catArrangementModel2 = new();
        //private static GLMesh catMesh;

        private static float Shininess = 50;

        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private const string TextureUniformVariableName = "uTexture";

        private const string LightColorVariableName = "lightColor";
        private const string LightPositionVariableName = "lightPos";
        private const string ViewPosVariableName = "viewPos";
        private const string ShininessVariableName = "shininess";


        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Garden Designer";
            windowOptions.Size = new Vector2D<int>(1000, 1000);

            // on some systems there is no depth buffer by default, so we need to make sure one is created
            windowOptions.PreferredDepthBufferBits = 24;

            cameraDescriptor = new CameraDescriptor(gardenerArrangementModel);
            externalCameraDescriptor = new ExternalCameraDescriptor(gardenerArrangementModel);
            plantIndexes = new int[20];
            plantMeshes =new GLMesh[20];
            trees = new ObjModel[20];
            treeScales = new float[20];
            plantCoords = new Tuple<int, int>[20];
            plantNames = new string[4];

            positionMatrix = new bool[40, 40];
            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 40; j++)
                {
                    if (i>1 && i< 39 && j>1 && j < 39)
                    {
                        positionMatrix[i, j] = true;
                    }
                    else
                    {
                        positionMatrix[i, j] = false;
                    }
                }
            }

            window = Window.Create(windowOptions);

            window.Load += Window_Load;
            window.Update += Window_Update;
            window.Render += Window_Render;
            window.Closing += Window_Closing;

            window.Run();
        }

        private static void Window_Load()
        {
            //Console.WriteLine("Load");

            // set up input handling
            inputContext = window.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
            }

            Gl = window.CreateOpenGL();

            controller = new ImGuiController(Gl, window, inputContext);

            // Handle resizes
            window.FramebufferResize += s =>
            {
                // Adjust the viewport to the new window size
                Gl.Viewport(s);
            };


            Gl.ClearColor(System.Drawing.Color.Black);

            SetUpObjects();

            LinkProgram();

            //Gl.Enable(EnableCap.CullFace);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);
        }

        private static void LinkProgram()
        {
            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, ReadShader("VertexShader.vert"));
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, ReadShader("FragmentShader.frag"));
            Gl.CompileShader(fshader);

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
        }

        private static string ReadShader(string shaderFileName)
        {
            using (Stream shaderStream = typeof(Program).Assembly.GetManifestResourceStream("GardenDesigner.Shaders." + shaderFileName))
            using (StreamReader shaderReader = new StreamReader(shaderStream))
                return shaderReader.ReadToEnd();
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.W:
                    cameraDescriptor.MoveForward(positionMatrix); 
                    gardenerArrangementModel.MoveForward(positionMatrix);
                    externalCameraDescriptor.MoveForward(positionMatrix);
                    break;
                case Key.S:
                    cameraDescriptor.MoveBackward(positionMatrix);
                    gardenerArrangementModel.MoveBackward(positionMatrix);
                    externalCameraDescriptor.MoveBackward(positionMatrix);
                    break;
                case Key.A:
                    cameraDescriptor.MoveLeft(positionMatrix);
                    gardenerArrangementModel.MoveLeft(positionMatrix);
                    externalCameraDescriptor.MoveLeft(positionMatrix);
                    break;
                case Key.D:
                    cameraDescriptor.MoveRight(positionMatrix);
                    gardenerArrangementModel.MoveRight(positionMatrix);
                    externalCameraDescriptor.MoveRight(positionMatrix);
                    break;
                case Key.E:
                    cameraDescriptor.MoveUp();  
                    break;
                case Key.Q:
                    cameraDescriptor.MoveDown();
                    break;
                case Key.Left:
                    cameraDescriptor.Rotate(-5f, 0); // Rotate left (yaw)
                    gardenerArrangementModel.RotateLeft(5f);
                    externalCameraDescriptor.Rotate(3.25f, 0);
                    break;
                case Key.Right:
                    cameraDescriptor.Rotate(5f, 0);  // Rotate right (yaw)
                    gardenerArrangementModel.RotateRight(5f);
                    externalCameraDescriptor.Rotate(-3.25f, 0);
                    break;
                case Key.Up:
                    cameraDescriptor.Rotate(0, 5f);  // Rotate up (pitch)
                    break;
                case Key.Down:
                    cameraDescriptor.Rotate(0, -5f); // Rotate down (pitch)
                    break;
                case Key.V:
                    externalCamera = !externalCamera;
                    break;
                case Key.Space:
                    catArrangementModel.AnimationEnabeld = true;
                    break;

            }
        }

        private static void Window_Update(double deltaTime)
        {
            //Console.WriteLine($"Update after {deltaTime} [s].");
            // multithreaded
            // make sure it is threadsafe
            // NO GL calls

            controller.Update((float)deltaTime);
        }

        private static unsafe void Window_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s].");

            // GL here
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);


            Gl.UseProgram(program);

            SetViewMatrix();
            SetProjectionMatrix();

            SetLightColor();
            SetLightPosition();
            SetViewerPosition();
            SetShininess();

            DrawWorld();

           // DrawRevolvingCube();

            DrawSkyBox();

            if (externalCamera)
            {
                DrawGardener();
            }
            DrawFence();
            DrawPlants();
            DrawCat();
            catArrangementModel.AdvanceTime(deltaTime, positionMatrix, 10);
            catArrangementModel2.AdvanceTime(deltaTime, positionMatrix, 4);

            ImGuiNET.ImGui.Begin("Camera", ImGuiNET.ImGuiWindowFlags.AlwaysAutoResize | ImGuiNET.ImGuiWindowFlags.NoCollapse);
            if (ImGuiNET.ImGui.RadioButton("External Camera", externalCamera))
            {
                externalCamera = true;
            }
            if(ImGuiNET.ImGui.RadioButton("First Person Camera", !externalCamera))
            {
                externalCamera = false;
            }
            ImGuiNET.ImGui.End();

            ImGuiNET.ImGui.Begin("Plant", ImGuiNET.ImGuiWindowFlags.AlwaysAutoResize | ImGuiNET.ImGuiWindowFlags.NoCollapse);
            if (ImGui.Combo("##actionCombo", ref selectedIndex, plantNames, plantNames.Length))
            {
                if (selectedIndex != 0)
                {
                    plantIndexes[plantLength] = selectedIndex-1;
                    var posX = cameraDescriptor.Position.X;
                    var posZ = cameraDescriptor.Position.Z;
                    var yaw = cameraDescriptor.Yaw - ((int)cameraDescriptor.Yaw/180)*180;
                    if (yaw <= -67.5 && yaw > -112.5)
                    {
                        plantCoords[plantLength] = new Tuple<int, int>((int)posX, (int)posZ-1);
                        positionMatrix[(int)posX + 20, (int)posZ + 19] = false;
                    }
                    else if (yaw <= -112.5 && yaw > -157.5)
                    {
                        plantCoords[plantLength] = new Tuple<int, int>((int)posX-1, (int)posZ-1);
                        positionMatrix[(int)posX + 19, (int)posZ + 19] = false;
                    }
                    else if (yaw <= -157 || yaw > 157.5)
                    {
                        plantCoords[plantLength] = new Tuple<int, int>((int)posX-1, (int)(posZ));
                        positionMatrix[(int)posX + 19, (int)posZ + 20] = false;
                    }
                    else if (yaw <= 157.5 && yaw > 112.5)
                    {
                        plantCoords[plantLength] = new Tuple<int, int>((int)posX - 1, (int)posZ + 1);
                        positionMatrix[(int)posX + 19, (int)posZ + 21] = false;
                    }
                    else if(yaw <= 112.5 && yaw > 67.5)
                    {
                        plantCoords[plantLength] = new Tuple<int, int>((int)posX, (int)posZ + 1);
                        positionMatrix[(int)posX + 20, (int)posZ + 21] = false;
                    }
                    else if(yaw <= 67.5 && yaw > 22.5)
                    {
                        plantCoords[plantLength] = new Tuple<int, int>((int)posX + 1, (int)posZ + 1);
                        positionMatrix[(int)posX + 21, (int)posZ + 21] = false;
                    }
                    else if(yaw <=22.5 && yaw > -22.5)
                    {
                        plantCoords[plantLength] = new Tuple<int, int>((int)posX + 1, (int)posZ);
                        positionMatrix[(int)posX + 21, (int)posZ + 20] = false;
                    }
                    else
                    {
                        plantCoords[plantLength] = new Tuple<int, int>((int)posX+1, (int)(posZ-1));
                        positionMatrix[(int)posX + 21, (int)posZ + 19] = false;
                    }
                    plantLength++;
                    selectedIndex = 0;
                }
            }
            ImGuiNET.ImGui.End();

            controller.Render();
        }

        private static unsafe void DrawSkyBox()
        {
            Matrix4X4<float> modelMatrix = Matrix4X4.CreateScale(400f);
            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(skyBox.Vao);

            int textureLocation = Gl.GetUniformLocation(program, TextureUniformVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
            }
            // set texture 0
            Gl.Uniform1(textureLocation, 0);

            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            Gl.BindTexture(TextureTarget.Texture2D, skyBox.Texture.Value);
            Gl.DrawElements(GLEnum.Triangles, skyBox.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            CheckError();
            Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
        }

        private static unsafe void DrawGardener()
        {
            var modelMatrixForGardener = Matrix4X4.CreateScale(0.5f);
            Matrix4X4<float> trans = Matrix4X4.CreateTranslation((float)gardenerArrangementModel.positionX, 0.7f, (float)gardenerArrangementModel.positionZ);
            Matrix4X4<float> rotx = Matrix4X4.CreateRotationX(0f);
            Matrix4X4<float> roty = Matrix4X4.CreateRotationY((float)gardenerArrangementModel.AngleY);
            modelMatrixForGardener = modelMatrixForGardener * roty * trans;
            SetModelMatrix(modelMatrixForGardener);
            gardenerMesh.Draw(Gl);
        }

        private static unsafe void DrawFence()
        {
            float posX = -19.25f;
            float posZ = -19.9f;
            
            Matrix4X4<float> rotx = Matrix4X4.CreateRotationX(-(float)Math.PI / 2);
            for (int i = 0; i < 10; i++)
            {
                var scale = Matrix4X4.CreateScale(0.0147f);
                var trans = Matrix4X4.CreateTranslation(posX, 0f, posZ);
                var modelMatrix = scale * rotx * trans;
                SetModelMatrix(modelMatrix);
                fenceMesh.Draw(Gl);
                posX += 3.98f;
            }
            Matrix4X4<float> roty = Matrix4X4.CreateRotationY((float)Math.PI / 2);
            posZ = -16.6f;
            posX = 19.75f;
            for (int i = 10; i < 20; i++)
            {
                var scale = Matrix4X4.CreateScale(0.0147f);
                var trans = Matrix4X4.CreateTranslation(posX, 0f, posZ);
                var modelMatrix = scale * rotx * roty * trans;
                SetModelMatrix(modelMatrix);
                fenceMesh.Draw(Gl);
                posZ += 3.98f;
            }
            posX = 16.6f;
            posZ = 19.75f;
            for (int i = 20; i < 30; i++)
            {
                var scale = Matrix4X4.CreateScale(0.0147f);
                var trans = Matrix4X4.CreateTranslation(posX, 0f, posZ);
                var modelMatrix = scale * rotx * trans;
                SetModelMatrix(modelMatrix);
                fenceMesh.Draw(Gl);
                posX -= 3.98f;
            }
            posX = -20f;
            posZ = 19.1f;
            for (int i = 30; i < 40; i++)
            {
                var scale = Matrix4X4.CreateScale(0.0147f);
                var trans = Matrix4X4.CreateTranslation(posX, 0f, posZ);
                var modelMatrix = scale * rotx * roty * trans;
                SetModelMatrix(modelMatrix);
                fenceMesh.Draw(Gl);
                posZ -= 3.98f;
            }
        }

        private static unsafe void DrawPlants()
        {
            plantIndexes[0] = 2;
            plantCoords[0] = new Tuple<int, int>(10, 12);
            positionMatrix[30, 32] = false;
            plantCoords[1] = new Tuple<int, int>(5, -4);
            plantIndexes[1] = 1;
            positionMatrix[25, 16] = false;

            for(int i = 0; i < plantLength; i++)
            {
                var scale = Matrix4X4.CreateScale(treeScales[plantIndexes[i]]);
                var trans = Matrix4X4.CreateTranslation(plantCoords[i].Item1, 0f, plantCoords[i].Item2);
                var modelMatrix = scale*trans;
                SetModelMatrix(modelMatrix);
                plantMeshes[plantIndexes[i]].Draw(Gl);
            }
        }

        private static unsafe void DrawCat()
        {
            var scale = Matrix4X4.CreateScale(0.001f);            
            var trans = Matrix4X4.CreateTranslation((float)catArrangementModel.positionX, 0f, (float)catArrangementModel.positionZ);
            var roty = Matrix4X4.CreateRotationY((float)catArrangementModel.AngleY-(float)Math.PI/2);
            var modelMatrix = scale*roty*trans;
            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(cat1.Vao);
            Gl.DrawElements(GLEnum.Triangles, cat1.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
            var scale2 = Matrix4X4.CreateScale(0.001f);
            var trans2 = Matrix4X4.CreateTranslation((float)catArrangementModel2.positionX, 0f, (float)catArrangementModel2.positionZ);
            var roty2 = Matrix4X4.CreateRotationY((float)catArrangementModel2.AngleY - (float)Math.PI / 2);
            var modelMatrix2 = scale2 * roty2 * trans2;
            SetModelMatrix(modelMatrix2);
            Gl.BindVertexArray(cat2.Vao);
            Gl.DrawElements(GLEnum.Triangles, cat2.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
            //catMesh.Draw(Gl);
        }


        private static unsafe void SetLightColor()
        {
            int location = Gl.GetUniformLocation(program, LightColorVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightColorVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, 1f, 1f, 1f);
            CheckError();
        }

        private static unsafe void SetLightPosition()
        {
            int location = Gl.GetUniformLocation(program, LightPositionVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightPositionVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, 0f, 10f, 0f);
            CheckError();
        }

        private static unsafe void SetViewerPosition()
        {
            int location = Gl.GetUniformLocation(program, ViewPosVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewPosVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, cameraDescriptor.Position.X, cameraDescriptor.Position.Y, cameraDescriptor.Position.Z);
            CheckError();
        }

        private static unsafe void SetShininess()
        {
            int location = Gl.GetUniformLocation(program, ShininessVariableName);

            if (location == -1)
            {
                throw new Exception($"{ShininessVariableName} uniform not found on shader.");
            }

            Gl.Uniform1(location, Shininess);
            CheckError();
        }
        private static unsafe void DrawWorld()
        {
            var modelMatrixForTable = Matrix4X4.CreateScale(1f, 1f, 1f);
            SetModelMatrix(modelMatrixForTable);
            Gl.BindVertexArray(ground.Vao);
            Gl.DrawElements(GLEnum.Triangles, ground.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2D, ground.Texture);

            int texLoc = Gl.GetUniformLocation(program, TextureUniformVariableName);
            Gl.Uniform1(texLoc, 0); // Use texture unit 0

            Gl.BindVertexArray(ground.Vao);
            Gl.DrawElements(GLEnum.Triangles, ground.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            Gl.BindTexture(TextureTarget.Texture2D, 0);

        }

        private static unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix)
        {
            int location = Gl.GetUniformLocation(program, ModelMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{ModelMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&modelMatrix);
            CheckError();

            var modelMatrixWithoutTranslation = new Matrix4X4<float>(modelMatrix.Row1, modelMatrix.Row2, modelMatrix.Row3, modelMatrix.Row4);
            modelMatrixWithoutTranslation.M41 = 0;
            modelMatrixWithoutTranslation.M42 = 0;
            modelMatrixWithoutTranslation.M43 = 0;
            modelMatrixWithoutTranslation.M44 = 1;

            Matrix4X4<float> modelInvers;
            Matrix4X4.Invert<float>(modelMatrixWithoutTranslation, out modelInvers);
            Matrix3X3<float> normalMatrix = new Matrix3X3<float>(Matrix4X4.Transpose(modelInvers));
            location = Gl.GetUniformLocation(program, NormalMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{NormalMatrixVariableName} uniform not found on shader.");
            }
            Gl.UniformMatrix3(location, 1, false, (float*)&normalMatrix);
            CheckError();
        }

        private static unsafe void SetUpObjects()
        {

            float[] face1Color = [1f, 0f, 0f, 1.0f];
            float[] face2Color = [0.0f, 1.0f, 0.0f, 1.0f];
            float[] face3Color = [0.0f, 0.0f, 1.0f, 1.0f];
            float[] face4Color = [1.0f, 0.0f, 1.0f, 1.0f];
            float[] face5Color = [0.0f, 1.0f, 1.0f, 1.0f];
            float[] face6Color = [1.0f, 0.5f, 0.0f, 1.0f];
            float[] face7Color = [0.2f, 0.2f, 0.2f, 1f];

            float[] tableColor = [System.Drawing.Color.Azure.R/256f,
                                  System.Drawing.Color.Azure.G/256f,
                                  System.Drawing.Color.Azure.B/256f,
                                  1f];
            ground = GlCube.CreateSquare(Gl, tableColor);
            var groundTexture = LoadTexture(Gl, "../../../Resources/ground.jpg");
            ground.Texture = groundTexture;
            skyBox = GlCube.CreateInteriorCube(Gl, "");
            gardener = ObjModel.LoadFromFile("../../../Resources/Lego/lego.obj");
            gardenerMesh = new GLMesh(Gl, gardener);
            fence = ObjModel.LoadFromFile("../../../Resources/Fence/Fence.obj");
            fenceMesh = new GLMesh(Gl, fence);
            plantNames[0] = "Select a plant";
            trees[0] = ObjModel.LoadFromFile("../../../Resources/Tree/Tree.obj");
            treeScales[0] = 0.8f;
            plantNames[1] = "Tree 1";
            trees[1] = ObjModel.LoadFromFile("../../../Resources/Tree 02/Tree.obj");
            treeScales[1] = 0.8f;
            plantNames[2] = "Tree 2";
            trees[2] = ObjModel.LoadFromFile("../../../Resources/rose/rose.obj");
            treeScales[2] = 0.01f;
            plantNames[3] = "Rose";
            for (int i = 0; i < 3; i++)
            {
                plantMeshes[i] = new GLMesh(Gl, trees[i]);
            }
            cat1 = ObjResourceReader.CreateObjectWithColor(Gl, face6Color, "GardenDesigner.Resources.cat.cat.obj");
            cat2 = ObjResourceReader.CreateObjectWithColor(Gl, face7Color, "GardenDesigner.Resources.cat.cat.obj");
            //cat = ObjModel.LoadFromFile("../../../Resources/cat/cat.obj");
            //catMesh = new GLMesh(Gl, cat);
        }



        private static void Window_Closing()
        {
           // teapot.ReleaseGlObject();
           // glCubeRotating.ReleaseGlObject();
           ground.ReleaseGlObject();
            skyBox.ReleaseGlObject();
        }

        private static unsafe void SetProjectionMatrix()
        {
            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)Math.PI / 4f, 1024f / 768f, 0.1f, 1000);
            int location = Gl.GetUniformLocation(program, ProjectionMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&projectionMatrix);
            CheckError();
        }

        private static unsafe void SetViewMatrix()
        {
            Matrix4X4<float> viewMatrix;
            if (!externalCamera)
            {
                viewMatrix = cameraDescriptor.GetViewMatrix();
            }
            else
            {
                viewMatrix = externalCameraDescriptor.GetViewMatrix();
            }
                int location = Gl.GetUniformLocation(program, ViewMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&viewMatrix);
            CheckError();
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }

        public static uint LoadTexture(GL gl, string path)
        {
            using var image = Image.Load<Rgba32>(path);
            image.Mutate(x => x.Flip(FlipMode.Vertical)); // Flip because OpenGL expects bottom-left origin

            var pixels = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixels);

            uint texture = gl.GenTexture();
            gl.BindTexture(TextureTarget.Texture2D, texture);
            unsafe
            {
                fixed (byte* ptr = pixels)
                {
                    gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba,
                        (uint)image.Width, (uint)image.Height, 0, PixelFormat.Rgba,
                        PixelType.UnsignedByte, ptr);
                }
            }

            gl.GenerateMipmap(TextureTarget.Texture2D);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);

            gl.BindTexture(TextureTarget.Texture2D, 0);
            return texture;
        }

    }
}