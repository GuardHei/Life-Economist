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
	public int maxGroundThickness = 2;
	public int minGroundThickness = 3;
	public Tilemap tilemap;
	public TileBase grass;
	public TileBase mud;
	public TileBase stone;
	public TileBase ice;
	public TileBase water;
	public TilePattern[] tilePatterns;

	public int length;
	public int height;
	public int groundThickness = 1;

	public bool g = true;

	private void Awake() {
		StartCoroutine(Exe());
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
		groundThickness = Random.Range(minGroundThickness, maxGroundThickness + 1);
		
		int[,] map = new int[length,height];
		for (int i = 0; i < length; i++)
			for (int j = 0; j < height; j++) map[i, j] = -1;
		
		for (int i = 0; i < length; i++)
			for (int j = 0; j < groundThickness; j++) map[i, j] = 1;

		int[] previousIndents = new int[maxJumpHeight];

		for (int i = 10; i < length - 20;) {
			int gap = Random.Range(5, 21);
			i += gap;
			int height = Random.Range(2, 10);
			int maxLength = height + Random.Range(2, 10);
			int lastLength = maxLength;
			int totalIndent = 0;
			for (int i0 = 0; i0 < maxJumpHeight; i0++) previousIndents[i0] = 1;
			for (int i1 = 0; i1 < height; i1++) {
				int indent = Random.Range(-3, 3);
				indent = indent < 0 ? 0 : indent;
				int times = 0;
				foreach (var pI in previousIndents) times += pI;
				if (times == 0) indent = 1;
				totalIndent += indent;
				int length = maxLength - i1 - Random.Range(0, 5) - indent;
				length = length < 1 ? 1 : length;
				if (length > lastLength) length = lastLength;
				lastLength = length;
				if (i + totalIndent + indent + length > this.length) length = 1;
				for (int i2 = totalIndent + indent; i2 < length; i2++) {
					map[i + i2, groundThickness + i1] = Random.Range(0, 10) < 8 ? 1 : 2;
				}
				for (int i3 = 0; i3 < maxJumpHeight - 1; i3++) previousIndents[i3] = previousIndents[i3 + 1];
				previousIndents[maxJumpHeight - 1] = indent;
			}
		}

		for (int i = 0; i < length; i++) {
			for (int j = 0; j < height - 1; j++) {
				if (map[i, j] == 1 && map[i, j + 1] == -1) map[i, j] = 0;
			}
		}

		for (int i = 0; i < length; i++)
			for (int j = 0; j < height; j++) tilemap.SetTile(new Vector3Int(i, j, 0), GetTile(map[i, j]));
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
	public bool needsFullContacts;
	public int leftHeight;
	public int rightHeight;
	public int[][] tiles;
}