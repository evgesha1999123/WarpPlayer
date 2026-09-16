create table if not exists PlaylistTrack (
	PlaylistId text not null,
	TrackId text not null,
	foreign key (PlaylistId) references Playlist(PlaylistId) on delete cascade,
	foreign key (TrackId) references Track(TrackId) on delete cascade
);