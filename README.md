# Community-driven-portal-for-video-game-reviews-and-ratings
Community-driven portal for video game reviews and ratings

To start the program you will first need to set up a database
to do this you will need a postgresql database with credentials as follows
Host=localhost; Port=5432; Database=games-platform; Username=postgres; Password=mysecretpassword
Docker command to run correct database:

docker run -d --name games-platform-db -e POSTGRES_DB=games-platform -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=mysecretpassword -p 5432:5432 postgres


