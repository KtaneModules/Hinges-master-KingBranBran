using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;
using System;

public class Hinges : MonoBehaviour {

	public KMSelectable module;
	public KMBombModule HingeModule;
	public KMAudio HingeAudio;
	public GameObject ModelComponent;
	public GameObject[] hingeGameObject;
	public KMSelectable[] hinges;
	public Material unicorn;
	public Rigidbody moduleRigidBody;
	public GameObject statusLight;
	private static int _moduleIdCounter = 1;
	private int _moduleId;
	private int[] hingeStatus = new int[8];
	private readonly string[] table = {"3", "8", "5", "7", "3", "4", "1", "6",  
									   "0", "1", "7", "2", "0", "9", "6", "8",  
									   "7", "9", "6", "9", "1", "1", "5", "1",  
									   "5", "8", "8", "0", "7", "7", "0", "6",  
									   "1", "7", "0", "3", "2", "9", "9", "5",  
									   "5", "8", "4", "4", "4", "2", "6", "2",  
									   "5", "2", "5", "0", "3", "4", "3", "2",  
									   "1", "6", "7", "4", "8", "3", "9", "0"};

	private readonly string[] table2 = {"23548671", "86732541", "26473518", "25734168", "76258413", "34856712", "71832546", "23546718", "15748632", "81475623"};
	private int currentHinge = 4;
	private int stage = 0;
	string[] correctButtonOrders;
	string[] correctButtonSolutions;
	private int hingesPressed = 0;
	private int amountHingesInit;
	private string input = "";
	private bool solved = false;
	private bool animationInProgress = false;

	void Awake()
    {
		_moduleId = _moduleIdCounter++;
		// The wierd stuff at end of condition is for unicorn.
		for (int i = 0; (hingeStatus.Sum() < 4 || hingeStatus.Sum() > 7) && (i < 1 || 0 != Random.Range(0, 50)); i++) {
			hingeStatus = new[] { Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2)};
		}

		if (hingeStatus.Sum() < 4 || hingeStatus.Sum() > 7) { // If there is rare unicorn, only 1 hinge.
			ModelComponent.GetComponent<MeshRenderer>().material = unicorn;
			hingeStatus = new[] {0, 0, 0, 0, 0, 0, 0, 0};
			hingeStatus[Random.Range(0, 8)] = 1;
		}

		correctButtonOrders = new string[hingeStatus.Sum()];
		correctButtonSolutions = new string[hingeStatus.Sum()];
		amountHingesInit = hingeStatus.Sum();

		GenerateSolution();
	}


	void Start()
	{
		for (int i = 0; i < hinges.Length; i++) 
		{
			var j = i;
			hinges[j].OnInteract += delegate { HingePressed(j); return false; };
		}
	}
	void Update()
	{

		for (int i = 0; i < hingeGameObject.Length; i++)
		{
			var hingeEnabled = hingeStatus[i] == 0 ? false : true;
			hingeGameObject[i].GetComponent<MeshRenderer>().enabled = hingeEnabled;
			hingeGameObject[i].GetComponent<BoxCollider>().enabled = hingeEnabled;
		}
	}

	void HingePressed(int hinge)
	{
		HingeAudio.PlaySoundAtTransform("HingeTap", hinges[hinge].transform);

		if (solved || animationInProgress)
			return;

		input += hinge + 1;

		if ((hinge + 1).ToString() == correctButtonSolutions[stage][hingesPressed].ToString())
		{
			
			hingesPressed++;
			
			if (hingesPressed == hingeStatus.Sum())
			{
				DebugLog("You pressed [{0}]. That is correct!", String.Join(", ", input.Select(x => x.ToString()).ToArray()));
				stage++;
				hingesPressed = 0;
				input = "";
				StartCoroutine(StagePass());
			}
		}
		else
		{
			DebugLog("You pressed [{0}]. That is wrong...", String.Join(", ", input.Select(x => x.ToString()).ToArray()));
			hingesPressed = 0;
			input = "";
			HingeModule.HandleStrike();
		}
	}

	private void GenerateSolution()
	{
		// Sad this doesn't work in log file analyzer :C 
		/*DebugLog("\n|-------------------|" +
				   "\n    HINGE {0} / {1}  " +
				   "\n|-------------------|", stage + 1, amountHingesInit);*/
		DebugLog("[]==[ HINGE {0} / {1} ]==[]", stage + 1, amountHingesInit);

		correctButtonOrders[stage] = FindButtonOrder();
		var buttonOrderString = correctButtonOrders[stage];
		var currentPresentHinges = hingeStatus.Select((hinge, ix) => hinge == 1 ? ix : -1).Where(x => x != -1).ToArray();
		DebugLog("The current hinges on the module are: {0}", String.Join(", ", currentPresentHinges.Select(x => (x + 1).ToString()).ToArray()));

		var tlHinge = buttonOrderString.IndexOf('0'); // X
		var trHinge = buttonOrderString.IndexOf('1'); // Y

		DebugLog("The top two hinge VALUES are: {0}, {1}", tlHinge+1, trHinge+1);

		// Find the value in the first table 
		var tableIx = trHinge * 8 + tlHinge;
		var tableTwoRow = int.Parse(table[tableIx]);
		var buttonPressOrder = table2[tableTwoRow];

		DebugLog("The hinges are to be pressed in this order: [{0}] => {1}", tableTwoRow, String.Join(", ", buttonPressOrder.Select(x => x.ToString()).ToArray()));

		correctButtonSolutions[stage] = String.Join("", buttonPressOrder.Select(x => x.ToString()).Where(x => currentPresentHinges.Contains(int.Parse(x) - 1)).ToArray());

		DebugLog("Press these hinges: {0}", String.Join(", ", correctButtonSolutions[stage].Select(x => x.ToString()).ToArray()));
	}

	private string FindButtonOrder()
	{
		string[] newNumberOrder = new string[8];
		int hingesFound = 0;
		var startingHinge = currentHinge;

		var foundHinge = false;

		// Keep going clockwise until a missing hinge is found.
		for (int i = 0; i < 8; i++)
		{
			var thisHinge = (startingHinge + i) % 8;

			if (hingeStatus[thisHinge] == 0)
			{
				currentHinge = thisHinge;
				foundHinge = true;
				break;
			}
		}
		if (!foundHinge)
			DebugLog("ERROR: I could not find a missing hinge on the module!");

		startingHinge = currentHinge;
		// First find the missing hinges.
		for (int i = 0; i < 8; i++)
		{
			var thisHinge = (startingHinge - i + 8) % 8; // '-' = ccw

			if (hingeStatus[thisHinge] == 0)
			{
				newNumberOrder[hingesFound] = thisHinge.ToString();
				hingesFound++;
				currentHinge = thisHinge;
			}
		}
		startingHinge = currentHinge;

		// Keep going counterclockwise until a present hinge is found.
		foundHinge = false;
		for (int i = 0; i < 8; i++)
		{
			var thisHinge = (startingHinge - i + 8) % 8;

			if (hingeStatus[thisHinge] == 1)
			{
				currentHinge = thisHinge;
				foundHinge = true;
				break;			
			}		
		}
		if (!foundHinge)
			DebugLog("ERROR: I could not find a present hinge after ordering the missing hinges!");

		startingHinge = currentHinge;

		// Then find the present hinges.
		for (int i = 0; i < 8; i++)
		{
			var thisHinge = (startingHinge + i) % 8; // '+' = cw

			if (hingeStatus[thisHinge] == 1)
			{
				newNumberOrder[hingesFound] = thisHinge.ToString();
				hingesFound++;
				currentHinge = thisHinge;
			}
		}

		DebugLog("The hinge order from 1 -> 8 is: {0}", String.Join(", ", newNumberOrder.Select(x => (int.Parse(x) + 1).ToString()).ToArray()));

		return String.Join("", newNumberOrder); // Turn the List<int> to a single string.
	}

	private IEnumerator StagePass()
	{
		animationInProgress = true;
		HingeAudio.PlaySoundAtTransform("HingeRip", module.transform);

		yield return new WaitForSeconds(.77f);

		// Remove a random hinge
		var currentPresentHinges = hingeStatus.Select((hg, ix) => hg == 1 ? ix : -1).Where(x => x != -1).ToArray();
		hingeStatus[currentPresentHinges[Random.Range(0, currentPresentHinges.Length)]] = 0;

		if (stage == amountHingesInit)
		{
			DebugLog("All the hinges are gone, so you solved the module!");
			solved = true;
			HingeAudio.PlaySoundAtTransform("Ding", module.transform);
			yield return new WaitForSeconds(.25f);
			moduleRigidBody.isKinematic = false;

			var x = module.transform.position.x + Random.Range(-1f, 1f); var y = module.transform.position.y + Random.Range(-1f, 1f); var z = module.transform.position.z + Random.Range(-1f, 1f);
			var randomNumbers = new[] { 5, 7, 9, -5, -7, -9 };
			moduleRigidBody.AddForceAtPosition(new Vector3(randomNumbers[Random.Range(0, 6)], randomNumbers[Random.Range(0, 6)], randomNumbers[Random.Range(0, 6)]), new Vector3(x, y, z));
			statusLight.SetActive(true);

			HingeModule.HandlePass();
			yield break;
		}
		
		GenerateSolution();
		animationInProgress = false;
	}

	private void DebugLog(string log, params object[] args)
	{
		var logData = string.Format(log, args);
		Debug.LogFormat("[Hinges #{0}] {1}", _moduleId, logData);
	}

	void TwitchHandleForcedSolve()
	{
		stage = amountHingesInit;
		StartCoroutine(ForceSolve());
	}

	private IEnumerator ForceSolve()
	{
		animationInProgress = true;
		HingeAudio.PlaySoundAtTransform("HingeRip", module.transform);

		yield return new WaitForSeconds(.77f);

		hingeStatus = new[] { 0, 0, 0, 0, 0, 0, 0, 0 };

		DebugLog("Force solving.");
		solved = true;
		HingeAudio.PlaySoundAtTransform("Ding", module.transform);
		yield return new WaitForSeconds(.25f);
		moduleRigidBody.isKinematic = false;

		var x = module.transform.position.x + Random.Range(-1f, 1f); var y = module.transform.position.y + Random.Range(-1f, 1f); var z = module.transform.position.z + Random.Range(-1f, 1f);
		var randomNumbers = new[] { 5, 7, 9, -5, -7, -9 };
		moduleRigidBody.AddForceAtPosition(new Vector3(randomNumbers[Random.Range(0, 6)], randomNumbers[Random.Range(0, 6)], randomNumbers[Random.Range(0, 6)]), new Vector3(x, y, z));
		statusLight.SetActive(true);

		HingeModule.HandlePass();
		yield break;
	}

	int TwitchModuleScore = 9;

	string TwitchHelpMessage = "Use '!{0} 1234 5 6 7 8' to press all the hinges.";

	IEnumerator ProcessTwitchCommand(string command)
	{
		var parts = command.ToCharArray().Where(c => c != ' ').ToArray();

		if (parts.All(x => "12345678".Contains(x) && hingeStatus[int.Parse(x.ToString()) - 1] != 0) ) {

			yield return null;

			for (int i = 0; i < parts.Count(); i++)
			{
				HingePressed(int.Parse(parts[i].ToString()) - 1);
				yield return new WaitForSeconds(.2f);
			}
			yield return new WaitForSeconds(.6f);
		}

		if (solved)
			yield return "solve";
	}
}
