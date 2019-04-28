using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour {

	public int minLength;
	public int maxLength;
	public int minHeight;
	public int maxHeight;
	public int maxJumpHeight = 2;
	public int minStandHeight = 3;
	public int minGroundThickness = 2;
	public int minHillGap = 1;
	public int maxHillGap = 11;
	public int minHillLength = 5;
	public int maxHillLength = 20;
	public int minHillHeight = 3;
	public int maxHillHeight = 10;
	public int skyLevel;
	public int waterLevel = 4;
	public int maxWaterLength = 8;
	public float waterChance = .7f;
	public Tilemap tilemap;
	public TileBase grass;
	public TileBase mud;
	public TileBase stone;
	public TileBase ice;
	public TileBase water;
	public TilePattern[] tilePatterns;

	public int length;
	public int height;
	public int[,] map;

	public bool g = true;

	private void Awake() {
		Generate();
		// StartCoroutine(Exe());
	}

	private IEnumerator Exe() {
		while (g) {
			Generate();
			yield return new WaitForSeconds(2);
		}
	}

	public void Generate() {
		tilemap.ClearAllTiles();
		length = Random.Range(minLength, maxLength + 1);
		height = Random.Range(minHeight, maxHeight + 1);
		
		// init
		map = new int[length,height];
		for (int i = 0; i < length; i++)
			for (int j = 0; j < height; j++) map[i, j] = -1;
		
		// fill min ground
		for (int i = 0; i < length; i++)
			for (int j = 0; j < minGroundThickness; j++) map[i, j] = 1;

		// squeeze hills
		bool exit = false;
		for (int i = 5; !exit;) {
			int gap = Random.Range(minHillGap, maxHillGap);
			i += gap;
			int l = Random.Range(minHillLength, maxHillLength);
			int h = Random.Range(minHillHeight, maxHillHeight);
			if (i + l >= length) {
				l = length - i;
				exit = true;
			}
			
			int indent = 0;
			int finishX = i + l;
			int indentCounts = 0;
			for (int i0 = minGroundThickness; i0 < h; i0++) {
				for (int i1 = i + indent; i1 < finishX; i1++) {
					map[i1, i0] = 1;
				}

				bool hasIndent = false;
				if (indentCounts < maxJumpHeight) hasIndent = Random.Range(0, 10) < 8;
				if (hasIndent) indent += Random.Range(1, 4);

				bool shorten = Random.Range(0, 10) < 7;
				if (shorten) l -= Random.Range(1, 3);
				if (l < 1) l = 1;

				finishX = i + l;
			}
		}
		
		// fill lakes
		for (int i = minGroundThickness; i < waterLevel; i++) {
			bool hasSolid = false;
			int startX = -1;
			for (int i0 = 0; i0 < length; i0++) {
				bool isSolid = map[i0, i] != -1;
				int type0 = map[i0, i - 1];
				int type1 = map[i0, i - 2];
				bool hasProblem = false;
				if (type0 == -1) hasProblem = true;
				else if (type0 == 5 && type1 == 5) hasProblem = true;
				if (hasProblem) {
					startX = -1;
					hasSolid = false;
					continue;
				}
				
				if (hasSolid && !isSolid && startX == -1) startX = i0;
				else if (isSolid && startX != -1) {
					int l = i0 - startX + 1;
					if (l <= maxWaterLength && Random.Range(0f, 1f) < waterChance)
						for (int i1 = startX; i1 < i0; i1++) map[i1, i] = 5;
					
					startX = -1;
					hasSolid = false;
				}

				if (isSolid) hasSolid = true;
			}
		}

		// hang platforms
		for (int i = skyLevel - 1; i < height - 1; i++) {
			
		}

		// grow grass
		for (int i = 0; i < length; i++) {
			for (int j = 0; j < height - 1; j++) {
				if (map[i, j] == 1 && map[i, j + 1] == -1) map[i, j] = 0;
			}
		}

		// generate tilemap
		for (int i = 0; i < length; i++)
			for (int j = 0; j < height; j++) tilemap.SetTile(new Vector3Int(i, j, 0), GetTile(map[i, j]));
		
		GC.Collect();
	}

	public TileBase GetTile(int type) {
		switch (type) {
			case 0: return grass;
			case 1: return mud;
			case 2: return stone;
			case 3: return stone;
			case 4: return ice;
			case 5: return water;
			default: return null;
		}
	}
}

[Serializable]
public class TilePattern {
	public string name;
	public bool initiated;
	public bool inAir;
	public bool onGround;
	public int leftHeight;
	public int rightHeight;
	public int bottomLength;
	[Multiline]
	public string arrangement;
	public int[,] tiles;

	public void Init() {
		if (initiated) return;

		string[] rows = arrangement.Split('\n');
		
		
		initiated = true;
	}
}