use std::time::Duration;
use spacetimedb::{rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp};

// We're using this table as a singleton, so in this table
// there only be one element where the `id` is 0.
#[spacetimedb::table(accessor = config, public)]
pub struct Config {
    #[primary_key]
    pub id: i32,
    pub world_size: i64,
}

// This allows us to store 2D points in tables.
#[derive(SpacetimeType, Clone, Debug)]
pub struct DbVector2 {
    pub x: f32,
    pub y: f32,
}

#[spacetimedb::table(accessor = entity, public)]
#[derive(Debug, Clone)]
pub struct Entity {
    // The `auto_inc` attribute indicates to SpacetimeDB that
    // this value should be determined by SpacetimeDB on insert.
    #[auto_inc]
    #[primary_key]
    pub entity_id: i32,
    pub position: DbVector2,
    pub mass: i32,
}

#[spacetimedb::table(accessor = circle, public)]
pub struct Circle {
    #[primary_key]
    pub entity_id: i32,
    #[index(btree)]
    pub player_id: i32,
    pub direction: DbVector2,
    pub speed: f32,
    pub last_split_time: Timestamp,
}

#[spacetimedb::table(accessor = food, public)]
pub struct Food {
    #[primary_key]
    pub entity_id: i32,
}

#[spacetimedb::table(accessor = player, public)]
#[derive(Debug, Clone)]
pub struct Player {
    #[primary_key]
    identity: Identity,
    #[unique]
    #[auto_inc]
    player_id: i32,
    name: String,
}

#[spacetimedb::reducer]
pub fn debug(ctx: &ReducerContext) -> Result<(), String> {
    log::debug!("This reducer was called by {}.", ctx.sender());
    Ok(())
}
