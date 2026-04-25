INSERT OR IGNORE INTO Level (id, level_name, mileage_goal, spawn_rate, boss_notice_text, arrival_notice_text, dlc_bundle_name)
VALUES (2, 'Level 2', 5000, 0.8, 'Dark Overlord Incoming!', 'You have arrived at Planet Void!', 'dlc_level2');

INSERT OR IGNORE INTO LevelNode (level_id, node_type, spawn_distance, sprite_key, variation_count, scale_min, scale_max, layer)
VALUES
(2, 'nebula_purple', 2000, 'dlc_nebula_dark',   1, 2.0, 3.5, -4),
(2, 'planet_rocky',  2000, 'dlc_planet_void',   2, 0.5, 0.5, -2),
(2, 'asteroid',      2200, 'dlc_asteroid_dark', 2, 0.3, 0.7, -1),
(2, 'planet_gas',    2500, 'dlc_planet_ember',  1, 0.5, 0.5, -3),
(2, 'nebula_blue',   3000, 'dlc_nebula_red',    1, 1.5, 3.0, -4),
(2, 'planet_rocky',  3500, 'dlc_planet_void',   2, 0.5, 0.5, -2),
(2, 'asteroid',      4000, 'dlc_asteroid_dark', 2, 0.2, 0.5, -1);