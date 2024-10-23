using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class PokemonAPIManager : MonoBehaviour
{
    public Transform contentTransform;  // Assign Content from ScrollView in the Inspector
    public GameObject pokemonPrefab;    // Assign Pokémon List Item prefab in the Inspector
    public TextMeshProUGUI pokemonNameText;  // Assign TextMeshPro field to display Pokémon name in detail view
    public TextMeshProUGUI pokemonStatsText; // Assign TextMeshPro field to display Pokémon stats in detail view
    public GameObject pokemonCardPanel;
    public Image pokemonImage;

    private string apiUrl = "https://pokeapi.co/api/v2/pokemon?limit=20";

    void Start()
    {
        // Start fetching the Pokémon list
        StartCoroutine(GetPokemonList(OnPokemonListReceived));
    }

    public IEnumerator GetPokemonList(System.Action<JArray> callback)
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error fetching data: " + request.error);
        }
        else
        {
            var jsonResponse = request.downloadHandler.text;
            Debug.Log("API Response: " + jsonResponse);  // Log API response

            var jsonParsed = JObject.Parse(jsonResponse);
            JArray pokemonList = (JArray)jsonParsed["results"];
            callback(pokemonList);  // Pass data to callback
        }
    }

    // Method to handle Pokémon list population
    void OnPokemonListReceived(JArray pokemonList)
    {
        foreach (var pokemon in pokemonList)
        {
            string pokemonName = pokemon["name"].ToString();    // Get the Pokémon's name
            string pokemonUrl = pokemon["url"].ToString();      // Get the Pokémon's detail URL

            // Instantiate a list item for each Pokémon
            GameObject listItem = Instantiate(pokemonPrefab, contentTransform);  // contentTransform is the Scroll View content
            listItem.GetComponentInChildren<TextMeshProUGUI>().text = pokemonName;  // Set the Pokémon name in the UI

            // Add an OnClick listener to the button for each list item
            listItem.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnPokemonClicked(pokemonUrl));
        }
    }

    // When a Pokémon is clicked, fetch its detailed info
    void OnPokemonClicked(string pokemonUrl)
    {
        StartCoroutine(GetPokemonDetails(pokemonUrl, ShowPokemonCard));
    }

    public IEnumerator GetPokemonDetails(string url, System.Action<JObject> callback)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error fetching data: " + request.error);
        }
        else
        {
            var jsonResponse = request.downloadHandler.text;
            JObject pokemonData = JObject.Parse(jsonResponse);
            callback(pokemonData); // Pass the parsed data to the callback
        }
    }

    void ShowPokemonCard(JObject pokemonData)
    {
        string name = pokemonData["name"].ToString();
        string height = pokemonData["height"].ToString();
        string weight = pokemonData["weight"].ToString();

        // Fetch the image URL from the sprite data
        string spriteUrl = pokemonData["sprites"]["front_default"].ToString();

        // Set the text and image in the Pokémon card
        pokemonNameText.text = $"Name: {name}";
        pokemonStatsText.text = $"Height: {height}\nWeight: {weight}";

        StartCoroutine(LoadSprite(spriteUrl));

        // Activate the Pokémon card panel
        pokemonCardPanel.SetActive(true);
    }

    private IEnumerator LoadSprite(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error fetching image: " + request.error);
            }
            else
            {
                // Get texture from the response and set it to the image component
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                pokemonImage.GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
    }

    public void HidePokemonDetailPanel()
    {
        pokemonCardPanel.SetActive(false);
    }
}
