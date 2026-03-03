# GinWhiskyExperten.com

GinWhiskyExperten is a REST API designed to help enthusiasts find bottles that match their specific taste preferences. By tracking flavors and their intensities, the system provides personalized recommendations and allows users to curate their own collection.

## User Flows
1. **Taste Discovery (GET):** Users can filter spirits by type (Gin/Whisky), alcohol percentage, and specific flavors (e.g., "Smokey" or "Floral"). Results are paginated for performance.
2. **Library Management (POST/PUT):** Administrators can add new bottles to the database, assign them to distilleries, and define their flavor profiles.
3. **Personalization (POST):** Users can rate bottles and save them to their personal "library" (wishlist).
4. **Smart Match (Logic):** Based on a specific bottle, the API suggests similar spirits by calculating the distance between flavor intensities.

=============================================================================================

<img width="6477" height="3525" alt="erdplus(6)" src="https://github.com/user-attachments/assets/07f358e8-f904-473a-ba1f-eeb319285d77" />


Main resource for browsing and managing the bottle collection.

Method	  Endpoint	        Description	                  Auth	    Params
GET   	/api/spirits	      Get all spirits (paginated)	  Public	  page, pageSize, type, minAbv
GET	    /api/spirits/{id}  	Get details for one spirit	  Public	  -
POST	  /api/spirits	      Create a new spirit	          Admin	    Body: SpiritDTO
PUT	    /api/spirits/{id}	  Update an existing spirit	    Admin	    Body: SpiritDTO
DELETE	/api/spirits/{id}	  Remove a spirit	              Admin	    -


Management of distilleries and brands.

Method	Endpoint	    Description	                Auth
GET	    /api/brands	  List all available brands	  Public
POST	  /api/brands	  Register a new brand	      Admin


Endpoints for user interaction and the recommendation engine.

Method	 Endpoint                  	         Description	                                Auth
POST	   /api/spirits/{id}/reviews	         Add a rating/comment to a bottle	            Public/User
GET	     /api/spirits/{id}/recommendations	 Get top 3 similar bottles based on flavor	  Public
