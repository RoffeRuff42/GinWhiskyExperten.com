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
| GET | /api/brands/{id} | Get details for one brand | Public |
| POST | /api/brands | Register a new brand | Admin |
| PUT | /api/brands/{id} | Update an existing brand | Admin |
| DELETE | /api/brands/{id} | Remove a brand (cascades to its spirits) | Admin |

### Flavor catalog and tasting profiles.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | /api/flavors | List all available flavors | Public |
| GET | /api/flavors/{id} | Get details for one flavor | Public |
| POST | /api/flavors | Register a new flavor | Admin |
| PUT | /api/flavors/{id} | Update an existing flavor | Admin |
| DELETE | /api/flavors/{id} | Remove a flavor | Admin |
| PUT | /api/spirits/{id}/flavors | Assign/update a flavor's intensity (1-5) on a bottle | Admin |
| DELETE | /api/spirits/{id}/flavors/{flavorId} | Remove a flavor assignment from a bottle | Admin |

### Endpoints for user interaction and the recommendation engine.

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | /api/spirits/{id}/recommendations | "Smart Match": top 3 spirits by flavor-profile similarity. Returns `[]` until the spirit has flavors assigned. | Public |
| POST | /api/spirits/{id}/reviews | Add a rating/comment to a bottle | **Planned** - not yet implemented |
| POST/GET | /api/library | Personal wishlist ("save to library") | **Planned** - requires public user accounts, not yet implemented |

### Authentication

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| POST | /api/auth/login | Exchange Admin credentials for a JWT (2h expiry) | Public, rate-limited (5/min) |

There is currently a single seeded **Admin** account (configured via `AdminUser:Email`/`AdminUser:Password`, see setup below) used to protect all write endpoints. Public user registration/login is part of the planned next round, alongside Reviews and the personal library.

========================================================================================================================================================

To build and run this API locally, follow these steps:
1. git clone https://github.com/RoffeRuff42/GinWhiskyExperten.com.git
2. dotnet ef database update
3. dotnet run

This project uses User Secrets to manage sensitive information. You need to configure the following secrets locally:
Right-click the project in Visual Studio and select Manage User Secrets.
Add the following JSON (generate your own random `Jwt:Key` - e.g. `openssl rand -base64 64` - and pick your own admin password; never commit real values):
<details>
  {
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GinWhiskeyDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "CocktailApiKey": "1",
  "Jwt": {
    "Key": "<a long random string, at least 32 characters>",
    "Issuer": "GinWhiskeyExperten",
    "Audience": "GinWhiskeyExperten"
  },
  "AdminUser": {
    "Email": "admin@example.com",
    "Password": "<a strong password - seeds the Admin account on first run>"
  }
}
</details>

Once running, navigate to https://localhost:[PORT]/swagger to explore the endpoints. To call an Admin-only endpoint from Swagger: `POST /api/auth/login` with your seeded admin credentials, then click **Authorize** and paste the returned token.

========================================================================================================================================================

## Performance Measurement
Measurement performed using Firefox Developer Tools (Network tab) on the `GET /api/spirits` endpoint.

| Scenario | Response Time | Description |
| :--- | :--- | :--- |
| **Cache Miss** | **78 ms** | Initial request. Data fetched from SQL Server. |
| **Cache Hit** | **6 ms** | Subsequent request. Data served instantly from IMemoryCache. |

**Result:** The cached response is approximately 13x faster than the initial database query.
