create table if not exists Playlist (
	PlaylistId text primary key,
	Name text,
	Description text null,
	CreatedAt integer,
	UpdatedAt integer
);