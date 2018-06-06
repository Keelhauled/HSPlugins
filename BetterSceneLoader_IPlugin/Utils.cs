using IllusionInjector;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UILib;
using UnityEngine;
using UnityEngine.UI;

namespace BetterSceneLoader
{
    static class Utils
    {
        public static void AddCloseSymbol(Button button)
        {
            var x1 = UIUtility.CreatePanel("x1", button.transform);
            x1.transform.SetRect(0f, 0f, 1f, 1f, 8f, 0f, -8f);
            x1.rectTransform.eulerAngles = new Vector3(0f, 0f, 45f);
            x1.color = new Color(0f, 0f, 0f, 1f);

            var x2 = UIUtility.CreatePanel("x2", button.transform);
            x2.transform.SetRect(0f, 0f, 1f, 1f, 8f, 0f, -8f);
            x2.rectTransform.eulerAngles = new Vector3(0f, 0f, -45f);
            x2.color = new Color(0f, 0f, 0f, 1f);
        }

        public static object InvokePluginMethod(string typeName, string methodName, params object[] parameters)
        {
            Type type = FindTypeIPlugin(typeName);

            if(type != null)
            {
                var instance = GameObject.FindObjectOfType(type);

                if(instance != null)
                {
                    parameters = parameters ?? new object[0];
                    Type[] paramTypes = parameters.Select(x => x.GetType()).ToArray();
                    BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                    MethodInfo methodInfo = type.GetMethod(methodName, bindingFlags, null, paramTypes, null);

                    if(methodInfo != null)
                    {
                        if(methodInfo.GetParameters().Length == 0)
                        {
                            return methodInfo.Invoke(instance, null);
                        }
                        else
                        {
                            return methodInfo.Invoke(instance, parameters);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Method {0}.{1} not found", typeName, methodInfo);
                    }
                }
                else
                {
                    Console.WriteLine("Instance of {0} not found", typeName);
                }
            }
            else
            {
                Console.WriteLine("Type {0} not found", typeName);
            }

            return null;
        }

        public static Type FindTypeIPlugin(string qualifiedTypeName)
        {
            Type t = Type.GetType(qualifiedTypeName);

            if(t != null)
            {
                return t;
            }
            else
            {
                foreach(Assembly asm in PluginManager.Plugins.Select(x => x.GetType().Assembly))
                {
                    t = asm.GetType(qualifiedTypeName);
                    if(t != null)
                    {
                        //Console.WriteLine("{0} belongs to an IPlugin", qualifiedTypeName);
                        return t;
                    }
                }

                foreach(Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    t = asm.GetType(qualifiedTypeName);
                    if(t != null)
                    {
                        return t;
                    }
                }

                return null;
            }
        }

        public static Texture2D LoadTexture(byte[] bytes)
        {
            Texture2D result;
            using (var memoryStream = new MemoryStream(bytes))
            {
                long num = 0L;
                PngAssist.CheckPngData(memoryStream, ref num, false);
                if (num == 0L)
                {
                    result = null;
                }
                else
                {
                    using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                    {
                        byte[] data = binaryReader.ReadBytes((int)num);
                        int num2 = 0;
                        int num3 = 0;
                        result = PngAssist.ChangeTextureFromPngByte(data, ref num2, ref num3);
                    }
                }
            }
            return result;
        }
    }
}
