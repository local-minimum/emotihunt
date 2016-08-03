using UnityEngine;
using System.Collections;

public class Feed : MonoBehaviour {

    public static DataFeed<FeedCard> Storage = new DataFeed<FeedCard>(Application.persistentDataPath + "feed.bin");

    [SerializeField]
    int index = 0;

    [SerializeField, Range(1, 10)]
    int readLength = 5;

	void Start () {
	}
	
	void Update () {
	
	}
}
