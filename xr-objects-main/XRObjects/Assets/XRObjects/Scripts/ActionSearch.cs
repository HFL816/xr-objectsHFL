// Copyright 2024 Google LLC

// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Class for querying general information about the object
// to be shown in the "information" panel

public class ActionSearch : ActionClass
{
  // public TextMeshProUGUI searchPanel;
  // public GameObject mainObjectProxy;
  [SerializeField] private GameObject infoDisplayPanel;
  private Button searchButton;
  private string queryResultText = "looking up...";
  private bool infoDisplayActive = false, questionQueryOngoing = false;
  private string transcribedUserPrompt;

  void Start()
  {
    searchButton = this.gameObject.GetComponent<Button>();
    searchButton.onClick.AddListener(showSearchResults);
  }

  private void showSearchResults()
  {
    // if (!infoDisplayActive)
    // {
    if (infoDisplayPanel.GetComponent<InfoPanelManager>().currentAction != this.gameObject || !infoDisplayPanel.GetComponentInChildren<Image>().enabled)
    {
      // close all submenus
      Component[] gridLayoutGroups = GetComponentInParent<RadialLayout>().gameObject.GetComponentsInChildren<GridLayoutGroup>();
      foreach (Component gridLayoutGroup in gridLayoutGroups)
      {
        gridLayoutGroup.gameObject.SetActive(false);
      }

      // add the "ask a question" button
      infoDisplayPanel.GetComponentInChildren<Button>(true).gameObject.SetActive(true);
      infoDisplayPanel.GetComponentInChildren<Button>(true).onClick.AddListener(startSpeechRecognition);
      infoDisplayPanel.GetComponentInChildren<Button>(true).GetComponentInChildren<TextMeshProUGUI>(true).text = "ask a question";

      infoDisplayPanel.GetComponentInChildren<Image>().enabled = true;
      infoDisplayPanel.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
      infoDisplayPanel.GetComponentInChildren<TextMeshProUGUI>().text = queryResultText;
    }
    else
    {

      infoDisplayPanel.GetComponentInChildren<Button>(true).gameObject.SetActive(false);
      infoDisplayPanel.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
      infoDisplayPanel.GetComponentInChildren<Image>().enabled = false;
    }

    infoDisplayPanel.GetComponent<InfoPanelManager>().currentAction = this.gameObject;
    infoDisplayActive = !infoDisplayActive;
  }

  private void startSpeechRecognition()
  {
    questionQueryOngoing = true;

    infoDisplayPanel.GetComponentInChildren<TextMeshProUGUI>().text = "listening...";
    infoDisplayPanel.GetComponentInChildren<Button>(true).gameObject.SetActive(false);
    GameObject.Find("SpeechRecognizer").GetComponent<SpeechRecognizer>().StartListeningAndDisplay(infoDisplayPanel.GetComponentInChildren<TextMeshProUGUI>(), this);
  }

  // onTranscriptionFinished is run after the user's prompt is transcribed for "ask a question"
  public override void onTranscriptionFinished(string speechTranscribedText)
  {
    transcribedUserPrompt = speechTranscribedText;
    
    infoDisplayPanel.GetComponentInChildren<TextMeshProUGUI>().text = "“<b>" + speechTranscribedText + "?</b>”\n\nthinking...";
    infoDisplayPanel.GetComponentInChildren<Button>(true).gameObject.SetActive(false);

    RunSearchCustomQuestion();
  }

  public void RunSearchCustomQuestion()
  {
    if(transcribedUserPrompt==""){
      questionQueryOngoing = false;
    }

    _ = StartCoroutine(GetComponentInParent<ImageQuery>().RunFollowUpImageQuery(transcribedUserPrompt, (result) =>
    {
      //Do something with the result variable
      Debug.Log("HTTP RunSearch result: " + result);
      queryResultText = result.ToString();
      infoDisplayPanel.GetComponentInChildren<TextMeshProUGUI>().text = queryResultText;

      infoDisplayPanel.GetComponentInChildren<Button>(true).gameObject.SetActive(true);
      infoDisplayPanel.GetComponentInChildren<Button>(true).GetComponentInChildren<TextMeshProUGUI>(true).text = "ask another question";

      questionQueryOngoing = false;

    }));

  }

  public static string AddNewlineBeforeStar(string input)
  {
    // check if the input string is null or empty
    if (string.IsNullOrEmpty(input))
      return input;

    // create a StringBuilder to manipulate the string
    System.Text.StringBuilder sb = new System.Text.StringBuilder(input);

    // iterate through the string to find '*' characters
    for (int i = 2; i < sb.Length; i++)
    {
      if (sb[i] == '*' && sb[i - 1] != '\n')
      {
        // Insert a newline character before '*'
        sb.Insert(i, '\n');

        // adjust the index to account for the newly inserted character
        i++;
      }
    }

    return sb.ToString();
  }


  public void RunSearch()
  {
    var prompt = "Can you give me the ones that make sense for this object? and fill in the missing …. using info from the Internet. Exclude the one that are irrelevant. Divide the relevant ones with a *. *: Price: (… give price+vendor+score/rating) * Cheaper alternatives: name - price * Main ingredients: … (top 2) * Calories: … * Allergens: … * Instructions: … (short) * Care: …(if fashion/tool/plant) * Extremely short answers. Exclude answers that are 'None' or 'n/a' or 'irrelevant'. Divide each with a *. Limit to 30 words";
    //* Assess health: health of plant (if plant).

    _ = StartCoroutine(GetComponentInParent<ImageQuery>().RunFollowUpImageQuery(prompt, (result) =>
    {
      //Do something with the result variable
      Debug.Log("HTTP RunSearch result: " + result);
      queryResultText = result.ToString();

      // remove "sure" or "here is..." statements
      if (queryResultText.ToLower().Contains("sure") || queryResultText.ToLower().Contains("here is "))
      {
        // split the string by the first '*' occurrence
        string[] parts = queryResultText.Split(new[] { '*' }, 2);

        // Get the last part after the last '*' character
        if (parts.Length > 1)
        {
          queryResultText = "*" + parts[1];
        }
      }

      queryResultText = queryResultText.Replace(":**", ":");
      queryResultText = queryResultText.Replace(":*", ":");
      queryResultText = queryResultText.Replace("\n\n* **", "\n");
      queryResultText = queryResultText.Replace("*\n* **", "\n");
      queryResultText = queryResultText.Replace("\n* **", "\n");

      queryResultText = queryResultText.Replace("** ", "*");

      // remove double stars
      queryResultText = queryResultText.Replace("*\n*", "\n*");

      // add new line before star in case it doesn't exist already
      Debug.Log("AddNewlineBeforeStar before - " + queryResultText);
      queryResultText = AddNewlineBeforeStar(queryResultText);
      Debug.Log("AddNewlineBeforeStar after - " + queryResultText);

      if (queryResultText.StartsWith(" "))
      {
        // remove the initial " " and assign the modified string
        queryResultText = queryResultText.Substring(1);
      }

      queryResultText = queryResultText.Replace("\n\n", "\n*");
      queryResultText = queryResultText.Replace("\n*\n*", "\n*");
      queryResultText = queryResultText.Replace("\n*\n*", "\n*");
      queryResultText = queryResultText.Replace("*\n*", "\n*");
      queryResultText = queryResultText.Replace("* \n*", "\n*");
      queryResultText = queryResultText.Replace("*  \n*", "\n*");

      // if string ends with *, remove it
      if (queryResultText.EndsWith("*"))
      {
        // Remove the '*' character from the end of the string
        queryResultText = queryResultText.Remove(queryResultText.Length - 1);
      }

      // if string starts with \n, remove it
      if (queryResultText.StartsWith("\n"))
      {
        // Remove the '\n' characters
        queryResultText = queryResultText.Substring(2);
      }

      // shorten the summary if it is too long
      if (queryResultText.Length > 120)
      {
        queryResultText = queryResultText.Substring(0, 120) + "…";
      }

      if(questionQueryOngoing){
        return;
      }

      if (infoDisplayPanel.GetComponent<InfoPanelManager>().currentAction == this.gameObject)
      {
        infoDisplayPanel.GetComponentInChildren<TextMeshProUGUI>().text = queryResultText;
      }


    }));
  
  }

}
