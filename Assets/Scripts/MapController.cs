using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour {
    public const float posF = 0.32f;
    public int width, height;
    public GameObject[] tilePrefabs;    //Prefab地图块文件
    public Transform boardHolder;       //确认在Hierarchy里边的存储层级

    private GameObject[,] tiles;
    private TileInfo[,] tileInfo;
    
    private class TileInfo
    {
        public Vector3 Position;
        public int TileID;

        public TileInfo(int x,int y,int ID)
        {
            this.Position = new Vector3(x, y);
            this.TileID = ID;
        }
    }
	//Starting Function
	void Start ()
    {
        //Load
        LoadRes(@"Prefabs\" + "Stage0" + 1.ToString());
        //Initialize map
        InitMap(1);

    }
    bool AddInstance()
    {
        try
        {

            return true;
        }
        catch(System.Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
    }
    void LoadRes(string Path)
    {
        tilePrefabs = Resources.LoadAll<GameObject>(Path);
        Debug.Log(">tilePrefabs Loaded " + tilePrefabs.Length.ToString() + " Elements");
    }
    void InitMap(int level)
    {
        switch(level)
        {
            case 1:
                width = UnityEngine.Random.Range(20, 40);
                height = UnityEngine.Random.Range(20, 40);
                break;
            default:
                Debug.Log(">Unknown level");
                break;
        }
		tiles = new GameObject[height, width];
		tileInfo = new TileInfo[height, width];
		GenMap(level);
		InstantiateMap();
	}
	void GenMap(int level)
	{
		switch(level)
		{
			case 1:
				for(int y=0;y< height;y++)
				{
					for(int x=0;x< width;x++)
					{
						SetTile(x, y, 1);
					}
				}
				BSP(0, 0, width - 1, height - 1);
				break;
			default:
				break;
		}
	}
	void BSP(int l,int b,int r,int t)
	{
		if ((r - l <= 6)||(t - b <= 6))
		{
			SetArea(l + 1, b + 1, r - 1, t - 1, 0);
			return;
		}
		int		sepLine;
		float	sepWay;
		float   rate;
		sepWay = Random.Range(0f, 1f);
		rate = ((float)t - b) / (t - b + r - l);
		//Debug.Log(rate);
		//Debug.Log((t - b).ToString() + " " + (r - l).ToString());
		//return;
		//if (sepWay < rate) 
		if ((t - b) < (r - l)) 
		{
			//纵切
			sepLine = Random.Range(l + 2, r - 1);
			BSP(l, b, sepLine, t);
			BSP(sepLine, b, r, t);
		}
		else
		{
			//横切
			sepLine = Random.Range(b + 2, t - 1);
			BSP(l, b, r, sepLine);
			BSP(l, sepLine, r, t);
		}
	}
	void SetTile(int x,int y,int TileID)
	{
		tiles[y, x] = tilePrefabs[TileID];
		tileInfo[y, x] = new TileInfo(x, y, TileID);
	}
	void SetArea(int l,int b,int r,int t,int TileID)
	{
		for(int x= l;x<= r;x++)
		{
			for(int y= b;y<= t;y++)
			{
				SetTile(x, y, TileID);
			}
		}
	}
    void InstantiateMap()
    {
        int x, y;
        for (x = 0; x < width; x++)
        {
            for (y = 0; y < height; y++)
            {
                GameObject g = Instantiate(tiles[y, x], tileInfo[y, x].Position * posF, Quaternion.identity) as GameObject;
				g.transform.SetParent(boardHolder);
				//tiles[y, x] = g;
			}
        }
    }

}
