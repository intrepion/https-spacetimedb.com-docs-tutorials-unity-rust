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
