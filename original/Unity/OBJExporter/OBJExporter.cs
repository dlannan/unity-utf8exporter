/*
Based on ObjExporter.cs, this "wrapper" lets you export to .OBJ directly from the editor menu.
 
This should be put in your "Editor"-folder. Use by selecting the objects you want to export, and select
the appropriate menu item from "Custom->Export". Exported models are put in a folder called
"ExportedObj" in the root of your Unity-project. Textures should also be copied and placed in the
same folder.
N.B. there may be a bug so if the custom option doesn't come up refer to this thread http://answers.unity3d.com/questions/317951/how-to-use-editorobjexporter-obj-saving-script-fro.html */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

//static Rect[] atlasUV
struct ObjMaterial
{
	public string name;
	public string textureName;
	public Texture	texture;
	public Vector2	textureUV;		// UV offset in Atlas
	public Vector2  textureSize;	// Size used in Atlas
}

public class OBJExporter : ScriptableObject
{
	// File copying by default for textures
	public static bool convertToPng = true;
	private static int vertexOffset = 0;
	private static int normalOffset = 0;
	private static int uvOffset = 0;
	
	
	//User should probably be able to change this. It is currently left as an excercise for
	//the reader.
	private static string targetFolder = "ExportedObj";

	// TODO: 
	// Develop a builtin UTF8 exporter. 
	static void WriteToUTF8(BinaryWriter writer, string data)
	{
		System.Text.Encoding utf_8 = System.Text.Encoding.UTF8;
		
		// Convert a string to utf-8 bytes.
		int charsUsed, bytesUsed;
		bool completed;
		byte[] utf8Bytes = new byte[data.Length];
		System.Text.Encoding.UTF8.GetEncoder().Convert(data.ToCharArray(), 0, (int)data.Length,
		                                               utf8Bytes, 0, data.Length, false, out charsUsed,
		                                               out bytesUsed, out completed);
		                      
		writer.Write(utf8Bytes);
	}

	private static string CollateMaterials(MeshFilter mf, Dictionary<string, ObjMaterial> materialList) 
	{
//		Mesh m = mf.sharedMesh;
//		Material[] mats = mf.renderer.sharedMaterials;
//		for (int material=0; material < m.subMeshCount; material ++) {
//			
//			foreach(Vector3 v in m.uv) 
//			{
//				sb.Append(string.Format("vt {0} {1}\n",v.x,v.y));
//			}
//
//			//See if this material is already in the materiallist.
//			try
//			{
//				ObjMaterial objMaterial = new ObjMaterial();
//				
//				objMaterial.name = mats[material].name;
//				
//				if (mats[material].mainTexture)
//				{
//					objMaterial.textureName = EditorUtility.GetAssetPath(mats[material].mainTexture);
//					objMaterial.texture = mats[material].mainTexture;
//				}
//				else 
//					objMaterial.textureName = null;
//				
//				materialList.Add(objMaterial.name, objMaterial);
//			}
//			catch (ArgumentException)
//			{
//				//Already in the dictionary
//			}
//		}
		return "";
	}

	private static string MeshToString(MeshFilter mf, Dictionary<string, ObjMaterial> materialList) 
	{
		Mesh m = mf.sharedMesh;
		Material[] mats = mf.gameObject.GetComponent<Renderer>().sharedMaterials;
		
		StringBuilder sb = new StringBuilder();
		
		sb.Append("g ").Append(mf.name).Append("\n");
		foreach(Vector3 lv in m.vertices) 
		{
			Vector3 wv = mf.transform.TransformPoint(lv);
			
			//This is sort of ugly - inverting x-component since we're in
			//a different coordinate system than "everyone" is "used to".
			sb.Append(string.Format("v {0} {1} {2}\n",-wv.x,wv.y,wv.z));
		}
		sb.Append("\n");
		
		foreach(Vector3 lv in m.normals) 
		{
			Vector3 wv = mf.transform.TransformDirection(lv);
			wv.Normalize();
			if((wv.x < 0.00001f) && (wv.x > -0.00001f)) wv.x = 0.0f;
			if((wv.y < 0.00001f) && (wv.y > -0.00001f)) wv.y = 0.0f;
			if((wv.z < 0.00001f) && (wv.z > -0.00001f)) wv.z = 0.0f;

			sb.Append(string.Format("vn {0} {1} {2}\n",-wv.x,wv.y,wv.z));
		}
		sb.Append("\n");
		
		foreach(Vector3 uv in m.uv) 
		{
			//uv.Normalize();
			sb.Append(string.Format("vt {0} {1}\n",uv.x,uv.y));
		}
		
		for (int material=0; material < m.subMeshCount; material ++) {
			sb.Append("\n");
			sb.Append("usemtl ").Append(mats[material].name).Append("\n");
			//sb.Append("usemap ").Append(mats[material].name).Append("\n");
			
			//See if this material is already in the materiallist.
			try
			{
				ObjMaterial objMaterial = new ObjMaterial();
				
				objMaterial.name = mats[material].name;
				
				if (mats[material].mainTexture)
				{
					objMaterial.textureName = EditorUtility.GetAssetPath(mats[material].mainTexture);
					objMaterial.texture = mats[material].mainTexture;
				}
				else 
				{
					objMaterial.textureName = null;
					objMaterial.texture = null;
				}
				
				materialList.Add(objMaterial.name, objMaterial);
			}
			catch (ArgumentException)
			{
				//Already in the dictionary
			}
			
			
			int[] triangles = m.GetTriangles(material);
			for (int i=0;i<triangles.Length;i+=3) 
			{
				//Because we inverted the x-component, we also needed to alter the triangle winding.
				sb.Append(string.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n", 
				                        triangles[i]+1 + vertexOffset, triangles[i+1]+1 + normalOffset, triangles[i+2]+1 + uvOffset));
			}

		}
		
		vertexOffset += m.vertices.Length;
		normalOffset += m.normals.Length;
		uvOffset += m.uv.Length;

		return sb.ToString();
	}
	
	private static void Clear()
	{
		vertexOffset = 0;
		normalOffset = 0;
		uvOffset = 0;
	}
	
	private static Dictionary<string, ObjMaterial> PrepareFileWrite()
	{
		Clear();
		return new Dictionary<string, ObjMaterial>();
	}
	
	public static void SetTextureImporterFormat( Texture2D texture, bool isReadable)
	{
		if ( null == texture ) return;

		string assetPath = AssetDatabase.GetAssetPath( texture );
		var tImporter = AssetImporter.GetAtPath( assetPath ) as TextureImporter;
		if ( tImporter != null )
		{
			tImporter.textureType = TextureImporterType.Advanced;

			tImporter.isReadable = isReadable;

			AssetDatabase.ImportAsset( assetPath );
			AssetDatabase.Refresh();
		}
	}	
	
	private static void MaterialsToFile(Dictionary<string, ObjMaterial> materialList, string folder, string filename)
	{
		using (StreamWriter sw = new StreamWriter(folder + "/" + filename + ".mtl")) 
		{
			foreach( KeyValuePair<string, ObjMaterial> kvp in materialList )
			{
				sw.Write("\n");
				sw.Write("newmtl {0}\n", kvp.Key);
				sw.Write("Ka  0.6 0.6 0.6\n");
				sw.Write("Kd  0.6 0.6 0.6\n");
				sw.Write("Ks  0.9 0.9 0.9\n");
				sw.Write("d  1.0\n");
				sw.Write("Ns  0.0\n");
				sw.Write("illum 2\n");
				
				if (kvp.Value.textureName != null)
				{
					string destinationFile = kvp.Value.textureName;
					
					
					int stripIndex = destinationFile.LastIndexOf('/');//FIXME: Should be Path.PathSeparator;
					
					if (stripIndex >= 0)
						destinationFile = "textures/" + destinationFile.Substring(stripIndex + 1).Trim();
								
					string relativeFile = destinationFile;
					
					try
					{
						// Generate a texture that is of a fixed type - PNG 32 bit ARGB.
						if(convertToPng == true)
						{
							string newdestination = destinationFile.Remove(destinationFile.IndexOf('.')) + ".png";
							
							relativeFile = newdestination;
							destinationFile = folder + "/" + newdestination;
					
							Texture2D sourceTex = (Texture2D)kvp.Value.texture;
							SetTextureImporterFormat(sourceTex, true);
							Texture2D destTex = new Texture2D(sourceTex.width,sourceTex.height);
							
							destTex.SetPixels32(sourceTex.GetPixels32());	
							destTex.Apply();

							byte[] bytes = destTex.EncodeToPNG();
							Debug.Log("Generating texture from " + bytes + " to " + destinationFile);
					
							File.WriteAllBytes(destinationFile, bytes);
						}
						// Use simple file copy
						else
						{
							relativeFile = destinationFile;
							destinationFile = folder + "/" + destinationFile;					
							Debug.Log("Copying texture from " + kvp.Value.textureName + " to " + destinationFile);
					
							//Copy the source file
							File.Copy(kvp.Value.textureName, destinationFile);
						}
					}
					catch
					{
						
					}	
										
					// Can potentially add more texture maps here if the material uses a standard shader.
					sw.Write("map_Kd {0}", relativeFile);
				}				
				sw.Write("\n\n\n");
			}
		}
	}
	
	private static void MeshToFile(MeshFilter mf, string folder, string filename) 
	{
		Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();

		// create a write stream
		// FileStream fs = new FileStream(folder +"/" + filename + ".utf8", FileMode.OpenOrCreate); 
		// using (BinaryWriter writeStream = new BinaryWriter(fs))
		{
			using (StreamWriter sw = new StreamWriter(folder +"/" + filename + ".obj")) 
			{
				sw.Write("mtllib ./" + filename + ".mtl\n");
			
				string outdata = MeshToString(mf, materialList);
				sw.Write(outdata);
				// WriteToUTF8(writeStream, outdata); 
			}
		}
		
		MaterialsToFile(materialList, folder, filename);
	}
	
	private static void MeshesToFile(MeshFilter[] mf, string folder, string filename) 
	{
		Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();
		
		// create a write stream
		//FileStream fs = new FileStream(folder +"/" + filename + ".utf8", FileMode.OpenOrCreate); 
		//using (BinaryWriter writeStream = new BinaryWriter(fs))
		{
			using (StreamWriter sw = new StreamWriter(folder +"/" + filename + ".obj")) 
			{
				sw.Write("mtllib ./" + filename + ".mtl\n");
			
				for (int i = 0; i < mf.Length; i++)
				{
					string outdata = MeshToString(mf[i], materialList);
					sw.Write(outdata);
					//WriteToUTF8(writeStream, outdata); 
				}
			}
		}

		MaterialsToFile(materialList, folder, filename);
	}
	
	private static bool CreateTargetFolder()
	{
		try
		{
			System.IO.Directory.CreateDirectory(targetFolder);
			System.IO.Directory.CreateDirectory(targetFolder + "/textures");
		}
		catch
		{
			EditorUtility.DisplayDialog("Error!", "Failed to create target folder!", "");
			return false;
		}
		
		return true;
	}
	
	[MenuItem ("Custom/Export/Export all MeshFilters in selection to separate OBJs")]
	static void ExportSelectionToSeparate()
	{
		if (!CreateTargetFolder())
			return;
		
		Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
		
		if (selection.Length == 0)
		{
			EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "");
			return;
		}
		
		int exportedObjects = 0;
		
		for (int i = 0; i < selection.Length; i++)
		{
			Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));
			
			for (int m = 0; m < meshfilter.Length; m++)
			{
				exportedObjects++;
				MeshToFile((MeshFilter)meshfilter[m], targetFolder, selection[i].name + "_" + i + "_" + m);
			}
		}
		
		if (exportedObjects > 0)
			EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects", "");
		else
			EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "");
	}


	[MenuItem ("Custom/Export/Export whole selection to single OBJ")]
	static void ExportWholeSelectionToSingle()
	{
		if (!CreateTargetFolder())
			return;
		
		
		Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
		
		if (selection.Length == 0)
		{
			EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "");
			return;
		}
		
		int exportedObjects = 0;
		
		ArrayList mfList = new ArrayList();
		
		for (int i = 0; i < selection.Length; i++)
		{
			Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));
			
			for (int m = 0; m < meshfilter.Length; m++)
			{
				exportedObjects++;
				mfList.Add(meshfilter[m]);
			}
		}
		
		if (exportedObjects > 0)
		{
			MeshFilter[] mf = new MeshFilter[mfList.Count];
			
			for (int i = 0; i < mfList.Count; i++)
			{
				mf[i] = (MeshFilter)mfList[i];
			}
			
			string filename = EditorApplication.currentScene; // + "_" + exportedObjects;
			
			int stripIndex = filename.LastIndexOf('/');//FIXME: Should be Path.PathSeparator
			
			if (stripIndex >= 0)
				filename = filename.Substring(stripIndex + 1).Trim();
			
			// REmove .unity extension
			filename = filename.Remove(filename.IndexOf('.'));
			
			MeshesToFile(mf, targetFolder, filename);


			
			EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects to " + filename, "");
		}
		else
			EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "");
	}
	
	
	[MenuItem ("Custom/Export/Export whole selection to single OBJ and Texture Atlas")]
	static void ExportWholeSelectionToSingleAtlas()
	{
		if (!CreateTargetFolder())
			return;

		Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
		
		if (selection.Length == 0)
		{
			EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "");
			return;
		}
		
		int exportedObjects = 0;
		
		ArrayList mfList = new ArrayList();

		// Build a list of meshFilters
		for (int i = 0; i < selection.Length; i++)
		{
			Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));
			
			for (int m = 0; m < meshfilter.Length; m++)
			{
				exportedObjects++;
				mfList.Add(meshfilter[m]);
			}
		}

		// Before exporting build a texture atlas (collect Materials) and keep UV offsets
		// for exporting.
		if (exportedObjects > 0)
		{

		}
		
		if (exportedObjects > 0)
		{
			MeshFilter[] mf = new MeshFilter[mfList.Count];
			
			for (int i = 0; i < mfList.Count; i++)
			{
				mf[i] = (MeshFilter)mfList[i];
			}
			
			string filename = EditorApplication.currentScene + "_" + exportedObjects;
			
			int stripIndex = filename.LastIndexOf('/');//FIXME: Should be Path.PathSeparator
			
			if (stripIndex >= 0)
				filename = filename.Substring(stripIndex + 1).Trim();
			
			MeshesToFile(mf, targetFolder, filename);
			
			
			EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects to " + filename, "");
		}
		else
			EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "");
	}

	[MenuItem ("Custom/Export/Export each selected to single OBJ")]
	static void ExportEachSelectionToSingle()
	{
		if (!CreateTargetFolder())
			return;
		
		Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
		
		if (selection.Length == 0)
		{
			EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "");
			return;
		}
		
		int exportedObjects = 0;
		
		
		for (int i = 0; i < selection.Length; i++)
		{
			Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));
			
			MeshFilter[] mf = new MeshFilter[meshfilter.Length];
			
			for (int m = 0; m < meshfilter.Length; m++)
			{
				exportedObjects++;
				mf[m] = (MeshFilter)meshfilter[m];
			}
			
			MeshesToFile(mf, targetFolder, selection[i].name + "_" + i);
		}
		
		if (exportedObjects > 0)
		{
			EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects", "");
		}
		else
			EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "");
	}
	
}