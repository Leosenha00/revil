using ReviveOS.CloudEngine;
if(args.Length!=2||args[1]!="--authorize-test-upload")throw new ArgumentException("Usage: <SSH alias> --authorize-test-upload; requires an authorized POSIX host with Python 3");
var root=Path.Combine(Path.GetTempPath(),"reviveos-ssh-integration-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
try{
 var input=Path.Combine(root,"test.py");await File.WriteAllTextAsync(input,"from pathlib import Path\nPath('result.json').write_text('{\"reviveos_test\":true}')\n");
 var runner=new JobRunner(Path.Combine(root,"jobs"));var job=await runner.Run("python",input,args[0],true);
 if(job.State!="completed"||await File.ReadAllTextAsync(job.Result!)!="{\"reviveos_test\":true}")throw new IOException("Unexpected remote result");
 Console.WriteLine("PASS SSH upload, Python execution, download and checksum; job "+job.Id);
 File.Delete(job.Result!); // Only the test-created local file, to exercise download recovery.
 var recovered=await runner.RetrieveRemoteResult(job.Id);
 if(recovered.State!="recovered-file"||await File.ReadAllTextAsync(recovered.Result!)!="{\"reviveos_test\":true}")throw new IOException("Recovery failed");
 Console.WriteLine("PASS confirmed remote result recovery and checksum");
 Console.WriteLine("Test files remain on authorized host at "+job.RemoteDirectory);
}finally{Directory.Delete(root,true);}
