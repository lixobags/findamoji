using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class WordBoard
{
	#region Data Classes

	public class WordTile
	{
		public bool used;			// Is this WordTile being used on the Board (ie. can a letter go here)
		public bool hasLetter;		// Is there a letter assigned to this WordTile
		public char letter;			// The letter that is assigned to this WordTile if hasLetter is true

		// The two variables below are only used by WordBoardCreator when generating the board.
		public int	region;
		public bool	regionLocked;
	}

	#endregion

	#region Enums

	public enum BoardState
	{
		Processing,
		DoneSuccess,
		DoneFailed,
		Restart
	}

	#endregion

	#region Member Variables

	public string			id;
	public int				size;		// The length of one side of a board (eg. a 7x7 board, the size would be 7)
	public string[]			words;		// The array of string words that are on this board
	public WordTile[]		wordTiles;	// The array of WordTiles that define what tiles the board uses, which ones have letters, and what the letters are
	public BoardState		boardState;	// The state of the board, used to communicate with the main thread

	public int				randSeed;	// Random seed used to create the rand
	public System.Random	rand;		// Used to get Random numbers for this board

	public Stopwatch		stopwatch;		// Used to keep track of how long the baord processing state is taking
	public long				restartTime;	// The amount of time in milliseconds thay must past for board creation to restart

	#endregion

	#region Public Methods

	/// <summary>
	/// Copies the board.
	/// </summary>
	public WordBoard Copy()
	{
		WordBoard newBoard = new WordBoard();

		newBoard.id			= id;
		newBoard.size		= size;
		newBoard.wordTiles	= new WordBoard.WordTile[newBoard.size * newBoard.size];

		for (int i = 0; i < newBoard.wordTiles.Length; i++)
		{
			WordBoard.WordTile wordTile			= wordTiles[i];
			WordBoard.WordTile newWordTile		= new WordBoard.WordTile();

			newWordTile.hasLetter		= wordTile.hasLetter;
			newWordTile.letter			= wordTile.letter;
			newWordTile.region			= wordTile.region;
			newWordTile.regionLocked	= wordTile.regionLocked;
			newWordTile.used			= wordTile.used;

			newBoard.wordTiles[i] = newWordTile;
		}

		return newBoard;
	}

	#endregion
}
