# GinWhiskyExperten.com

GinWhiskyExperten is a REST API designed to help enthusiasts find bottles that match their specific taste preferences. By tracking flavors and their intensities, the system provides personalized recommendations and allows users to curate their own collection.

## User Flows
1. **Taste Discovery (GET):** Users can filter spirits by type (Gin/Whisky), alcohol percentage, and specific flavors (e.g., "Smokey" or "Floral"). Results are paginated for performance.
2. **Library Management (POST/PUT):** Administrators can add new bottles to the database, assign them to distilleries, and define their flavor profiles.
3. **Personalization (POST):** Users can rate bottles and save them to their personal "library" (wishlist).
4. **Smart Match (Logic):** Based on a specific bottle, the API suggests similar spirits by calculating the distance between flavor intensities.

========================================================================================================================================================

<img width="800" height="3525" alt="erdplus(6)" src="https://github.com/user-attachments/assets/07f358e8-f904-473a-ba1f-eeb319285d77" />


### Spirits Resource
Main resource for browsing and managing the bottle collection.

| Method | Endpoint | Description | Auth | Params |
| :--- | :--- | :--- | :--- | :--- |
| GET | /api/spirits | Get all spirits (paginated) | Public | page, pageSize, type, minAbv |
| GET | /api/spirits/{id} | Get details for one spirit | Public | - |
| POST | /api/spirits | Create a new spirit | Admin | Body: SpiritDTO |
| PUT | /api/spirits/{id} | Update an existing spirit | Admin | Body: SpiritDTO |
| DELETE | /api/spirits/{id} | Remove a spirit | Admin | - |


### Management of distilleries and brands.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | /api/brands | List all available brands | Public |
| POST | /api/brands | Register a new brand | Admin |


### Endpoints for user interaction and the recommendation engine.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| POST | /api/spirits/{id}/reviews | Add a rating/comment to a bottle | Public/User |
| GET | /api/spirits/{id}/recommendations | Get top 3 similar bottles based on flavor | Public |

========================================================================================================================================================

To build and run this API locally, follow these steps:
1. git clone https://github.com/RoffeRuff42/GinWhiskyExperten.com.git
2. dotnet ef database update
3. dotnet run

This project uses User Secrets to manage sensitive information. You need to configure the following secrets locally:
Right-click the project in Visual Studio and select Manage User Secrets.
Add the following JSON: 
<details>
  {
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GinWhiskeyDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "CocktailApiKey": "1"
}
</details>

Once running, navigate to https://localhost:[PORT]/swagger to explore the endpoints.

========================================================================================================================================================

## Performance Measurement
Measurement performed using Firefox Developer Tools (Network tab) on the `GET /api/spirits` endpoint.

| Scenario | Response Time | Description |
| :--- | :--- | :--- |
| **Cache Miss** | **78 ms** | Initial request. Data fetched from SQL Server. |
| **Cache Hit** | **6 ms** | Subsequent request. Data served instantly from IMemoryCache. |

**Result:** The cached response is approximately 13x faster than the initial database query.
