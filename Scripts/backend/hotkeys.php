<?php // hotkeys.php

$unity = true; // required to interact with storage.php
$login = true; // required to interact with storage.php

require_once '../../keys/storage.php';

if (!isset($_POST["action"]) || !isset($_POST["username"])) {
    error_log("Missing POST data");
    exit();
}

$action = $_POST["action"];
$playerUsername = $_POST["username"];
$hotkeysData = isset($_POST["hotkeys"]) ? $_POST["hotkeys"] : null;

// Prepare statement to get player ID
$stmt = $conn->prepare("SELECT id FROM players WHERE username = ?");
$stmt->bind_param('s', $playerUsername);
$stmt->execute();
$result = $stmt->get_result();

if ($result->num_rows != 1) {
    echo "No user found";
    exit();
}

$playerId = $result->fetch_assoc()["id"];

// Prepare statement to check if player has an entry in player_stats
$stmt = $conn->prepare("SELECT COUNT(*) as cnt FROM player_stats WHERE player_id = ?");
$stmt->bind_param('i', $playerId);
$stmt->execute();
$result = $stmt->get_result();
$count = $result->fetch_assoc()["cnt"];

if ($action === "save") {
    if ($hotkeysData === null) {
        echo "Missing hotkeys data";
        exit();
    }

    if ($count > 0) {
        $stmt = $conn->prepare("UPDATE player_stats SET hotkeys = ? WHERE player_id = ?");
        $stmt->bind_param('si', $hotkeysData, $playerId);
    } else {
        $stmt = $conn->prepare("INSERT INTO player_stats (player_id, hotkeys) VALUES (?, ?)");
        $stmt->bind_param('is', $playerId, $hotkeysData);
    }

    if ($stmt->execute() === TRUE) {
        echo "Hotkeys data saved";
    } else {
        echo "Error saving hotkeys data";
    }
} elseif ($action === "load") {
    if ($count > 0) {
        $stmt = $conn->prepare("SELECT hotkeys FROM player_stats WHERE player_id = ?");
        $stmt->bind_param('i', $playerId);
        $stmt->execute();
        $result = $stmt->get_result();
        echo $result->fetch_assoc()["hotkeys"];
    } else {
        echo "No hotkeys data found";
    }
} else {
    echo "Invalid action";
}

$conn->close();

?>
