using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Linq;
using System.Reflection;

namespace BlitzedWebhookDecrypter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ModuleDefMD Module = ModuleDefMD.Load(args[0]);
            Assembly asm = Assembly.LoadFile(args[0]);
            foreach (TypeDef type in Module.Types.Where(t => t.HasMethods))
            {
                foreach (MethodDef method in type.Methods.Where(m => m.HasBody && m.Body.HasInstructions))
                {
                    for (int i = 0; i < method.Body.Instructions.Count(); i++)
                    {
                        try
                        {
                            //Identify Decryption Method
                            if (method.Body.Instructions[i].OpCode == OpCodes.Call && method.Body.Instructions[i].Operand.ToString().Contains("Unniggerify(System.String,System.String)"))
                            {
                                //Take Operand(Called Method) As MethodDef
                                MethodDef decryptMethod = method.Body.Instructions[i].Operand as MethodDef;

                                //Invoke decryptMethod With The Needed Args To Get Webhook Back
                                string encryptedWebhook = method.Body.Instructions[i - 2].Operand.ToString();
                                string passPhrase = method.Body.Instructions[i - 1].Operand.ToString();
                                string decryptedWebhook = (string)((MethodInfo)asm.ManifestModule.ResolveMethod((int)decryptMethod.MDToken.Raw)).Invoke(null, new object[] { encryptedWebhook, passPhrase });

                                /*Replace With Real Webhook*/
                                method.Body.Instructions.RemoveAt(i);
                                method.Body.Instructions[i - 1].OpCode = OpCodes.Ldstr;
                                method.Body.Instructions[i - 1].Operand = decryptedWebhook;
                                method.Body.Instructions.RemoveAt(i - 2);
                                /*Replace With Real Webhook*/

                                Console.WriteLine($"Decrypted Webhook: {decryptedWebhook}");
                            }
                        }
                        catch (Exception ex)
                        {
                            //Shhht it never goes there
                        }
                    }
                }
            }
            Module.Write(Module.Name.Replace(".exe", "-sussyGrabber.exe")); //Write New Module With Decrypted Webhook
            Console.ReadLine();
        }
    }
}