using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

using System;

public class OpenAIAgent : MonoBehaviour
{
    [TextArea(2, 10)]
    [Header("OpenAI API Settings")]
    public string _systemPrompt = @"
        Simulate the update rule of a cellular automata based game set in a medieval time, like a medieval SimCity.
        I'll give you the JSON representations of the cells surrounding a specific cell (the 8 cardinal directions), and the representation of the central cell we want to update at the previous timestep and you should generate the representation of the central cell for the next timestep.
        Each timestep should represent a week elapsing. When reasonable, incorporate what is happening in neighboring cells.
        Focus on the event that should happen first, then proliferate the rest of the values.

        # Rules

        - cells can change type (for isntance a forest on fire can become a plain, a farm fiels that has been harvested can become a plain, a castle that has been attacked can become a ruin, a village can grow into a castle)
        - you can fortify the central cell only if there are enough stones in all the cells at the previous timestep
        - `type` and `icon` can only have one selected value
        - `status` can have at most 2 at a time
        - `history` should be limited to 3 sentences.
        - `resources` must range from 0 to 1000
        - `description` should be evocative as well as hint at mechanical relevance (i.e. is bountiful, is void of life, etc.)
        - caves extinguish after having been mined of their materials
        - villages can promote to castles when lots of food is available, population increases and lots of stone is available

        # Cell JSON Schema

        {
        \'event': str (a short textual description of what happened in this cell that led to the change from the previous state to the current state),
        'type': 'forest | farmland | village | castle | ruins | bandit_camp | river | plain | hill | mountain | cave',
        'description': str (A very short, evocative text about this spot),
        'icon': '🌳 | 🌱 | 🏘️ | 🏰 | 💀 | ⚔️ | 🌊 | 🌾 | ⛰️ | 🪨 | 🕳️',
        'population': int,
        'resources': {
        'food': int,
        'wood': int,
        'stone': int
        },
        'morale': int,
        'status': ['storm', 'fire', 'fortified', 'plague', 'flood', 'drought', 'sieged'],
        'history': str (history of this cell)
        }


        # Example Cells

        Peaceful River
        {
        'type': 'river',
        'description': 'A serene river winding through the plains.',
        'icon': '🌊',
        'population': 0,
        'resources': {
        'food': 8,
        'wood': 2,
        'stone': 0
        },
        'morale': 50,
        'status': [],
        'event': '',
        'history': 'many armies crossed this river'
        }


        Castle Under Siege
        {
        'type': 'castle',
        'description': 'A stronghold perched atop a craggy mountain, smoke rising from its walls.',
        'icon': '🏰',
        'population': 15,
        'resources': {
        'food': 5,
        'wood': 3,
        'stone': 20
        },
        'morale': 25,
        'status': ['sieged', 'fire'],
        'event': 'Flames engulf the outer walls as enemy troops attack.',
        'history': 'this is where the king Leo XI was born'
        }
        ";

    public string apiKey = "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"; // Replace with your actual OpenAI API key
    private string endpoint = "https://api.openai.com/v1/chat/completions";

    [Header("Response")]
    [TextArea(2, 10)]
    public string responseText; // Output from OpenAI stored here

    // Call this method to start sending a prompt (e.g., from a UI button or Start)
    void Start()
    {

    }

    public IEnumerator SendPrompt(string prompt)
    {
        // Compose Chat Messages
        var messages = new List<OpenAIMessage>
        {
            new OpenAIMessage { role = "system", content = _systemPrompt },
            new OpenAIMessage { role = "user", content = prompt }
        };

        ChatRequest chatRequest = new ChatRequest
        {
            model = "gpt-4o", // or another model if needed
            messages = messages,
            max_tokens = 400,
            temperature = 0.2f
        };

        string jsonData = JsonUtility.ToJson(chatRequest);

        using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError("OpenAI API Error: " + request.error);
            }
            else
            {
                string responseString = request.downloadHandler.text;
                ChatResponse chatResponse = JsonUtility.FromJson<ChatResponse>(responseString);

                if (chatResponse.choices != null && chatResponse.choices.Count > 0)
                {
                    responseText = chatResponse.choices[0].message.content;
                    Debug.Log("OpenAI Response: " + responseText);
                }
                else
                {
                    Debug.LogWarning("No response choices received from OpenAI.");
                }
            }
        }
    }
}

[Serializable]
public class OpenAIMessage
{
    public string role;
    public string content;
}

[Serializable]
public class ChatRequest
{
    public string model;
    public List<OpenAIMessage> messages;
    public int max_tokens = 200;
    public float temperature = 0.2f;
}

[Serializable]
public class ChatChoice
{
    public int index;
    public OpenAIMessage message;
    public string finish_reason;
}

[Serializable]
public class ChatResponse
{
    public string id;
    public string @object;
    public int created;
    public List<ChatChoice> choices;
    public object usage;
}


public class PromptHandler
{
    public readonly OpenAIAgent openAIAgent;

    private static readonly string[] Directions = new[]
    {
        "North West", "North", "North East",
        "West",                      "East",
        "South West", "South", "South East"
    };

    private readonly string userPromptTemplate = @"
# Surrounding Cells at previous timestep
{0}

# Current central Cell at previous timestep
{1}

# Task
Generate ONLY the JSON of ONLY ONE CELL, the current central cell according to the cell JSON Schema.";

    public PromptHandler()
    {
        this.openAIAgent = new OpenAIAgent();
    }

    public IEnumerator SendPromptWithList(GameObject centralCell, List<GameObject> neighborCells)
    {
        var formattedNeighbors = FormatNeighborCells(neighborCells);
        string userPrompt = string.Format(userPromptTemplate, formattedNeighbors, centralCell.GetComponent<Cell>().ToJson());
        Debug.Log($"[SendPromptWithList] userPrompt:\n{userPrompt}");
        
        yield return openAIAgent.SendPrompt(userPrompt);
    }

    private string FormatNeighborCells(List<GameObject> neighbors)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < 8; i++)
        {
            if (neighbors[i] != null)
            {
                sb.AppendLine(Directions[i]);
                sb.AppendLine(neighbors[i].GetComponent<Cell>().ToJson());
                sb.AppendLine();
            }
        }
        return sb.ToString().Trim();
    }
    

    // public void DebugSelf()
    // {
    // List<string> testList = new List<string>
    // {
    // @"{ 'type': 'plain',
    //     'description': 'Flat land',
    //     'resources': { 'food': 2 }
    //     }",
    // @"{ 'type': 'village',
    //     'description': 'Small village',
    //     'resources': { 'food': 10, 'wood': 5 } }",
    // @"{ 'type': 'river', 
    //     'description': 'A calm river', 
    //     'resources': { 'food': 5, 'stone': 1 } }",
    // @"{ 'type': 'hill', 
    //     'description': 'Rolling hill', 
    //     'resources': { 'stone': 4 } }",
    // @"{ 'type': 'forest', 
    //     'description': 'Dense woods', 
    //     'resources': { 'wood': 15 } }",
    // @"{ 'type': 'mountain', 
    //     'description': 'Tall mountain', 
    //     'resources': { 'stone': 20 } }",
    // @"{ 'type': 'bandit_camp', 
    //     'description': 'Shady camp', 
    //     'resources': { 'food': 1 } }",
    // @"{ 'type': 'farmland', 
    //     'description': 'Crop-filled land', 
    //     'resources': { 'food': 8 } }"
    // };

    // string center = @"{
    //     'type': 'forest',
    //     'description': 'Thick trees',
    //     'resources': { 'wood': 20 }
    // }";

    // Debug.Log("PromptHandler.DebugSelf() running...");
    // SendPromptWithList(center, testList);
    // }
}




// Example usage
// string userPromptTemplate = "Center object: {0}\n\nSurrounding:\n{1}";
// var handler = new PromptHandler(systemPrompt, userPromptTemplate);
// List of 8 elements, some null
// var surroundings = new List<object>
// {
//     "Rock", null, "House",
//     "Wall", "Path", null
//     null, "River", "Field"
// };
// handler.SendPromptWithList("Tower", surroundings);