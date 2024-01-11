using UnityEngine;
using UnityEditor;
using System.Collections;

public class AudioImportSettings : AssetPostprocessor
{
    void OnPostprocessAudio(AudioClip ac)
    {
        AudioImporter import = (AudioImporter)assetImporter;
        if (ac)
        {
            //AudioImporterSampleSettings aiss = import.defaultSampleSettings;
            //aiss.loadType = ac.length > 5f ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
            //import.forceToMono = true;
            //import.defaultSampleSettings = aiss;
        }
    }
}
