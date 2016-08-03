using UnityEngine;
using System.Linq;

public class Feed : MonoBehaviour {

    public static DataFeed<FeedCard> Storage = new DataFeed<FeedCard>(Application.persistentDataPath + "/feed.bin");

    [SerializeField]
    int index = 0;

    [SerializeField, Range(1, 10)]
    int readLength = 5;

	void Start () {
        int l = Storage.Count;
        index = Mathf.Max(0, l - readLength);
        Debug.Log("Feed length: " + l);
        Debug.Log("Feed at: " + index);
        
        LoadBatch();
    }
	
	void Update () {
	
	}

    void LoadBatch()
    {
        try
        {
            var newPosts = Storage.Read(index, readLength);
            Debug.Log("Loaded: " + newPosts.Count);

        }
        catch (System.IO.FileNotFoundException)
        {
            Debug.LogWarning("No feed save data");
        } catch (System.DataMisalignedException)
        {
            Debug.LogError("Feed is corrupt, will be wiped");
        }
    }


}
