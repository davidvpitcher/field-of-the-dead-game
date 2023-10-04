<?php // saveorupdate.php
$unity = true; // required for the storage keys to function and get the right keys
$login = true; // required for the storage keys to function and get the right keys

$response = array(); // This will store response data

require_once '../../keys/storage.php'; // this grants pdo and conn

// Check if POST variables are set
if (!isset($_POST['playerID']) || !isset($_POST['achievementID']) || !isset($_POST['isCompleted']) || !isset($_POST['completionDate'])) {
    $response["status"] = "error";
    $response["message"] = "Missing required POST variables.";
    echo json_encode($response);
    exit();
}

$playerID = $_POST['playerID'];
$achievementID = $_POST['achievementID'];
$isCompleted = $_POST['isCompleted'] === '1' ? true : ($_POST['isCompleted'] === '0' ? false : null);

if ($isCompleted === null) {
    $response["status"] = "error";
    $response["message"] = "Invalid input. isCompleted should be '1' for true or '0' for false.";
    echo json_encode($response);
    exit();
}

$completionDate = $_POST['completionDate'];

// Input Validation
if (!is_numeric($playerID) || !is_numeric($achievementID) || !is_bool($isCompleted) || !strtotime($completionDate)) {
    $response["status"] = "error";
    $response["message"] = "Invalid input.";
    echo json_encode($response);
    exit();
}

// Check if the record exists
$stmt = $pdo->prepare("SELECT id FROM player_achievements WHERE playerID = ? AND achievementID = ?");
$stmt->execute([$playerID, $achievementID]);
$row = $stmt->fetch();

if ($row) {
    // Update the achievement if it exists
    $stmt = $pdo->prepare("UPDATE player_achievements SET isCompleted = ?, completionDate = ? WHERE id = ?");
    if ($stmt->execute([$isCompleted, $completionDate, $row['id']])) {
        $response["status"] = "success";
        $response["action"] = "updated";
        $response["message"] = "Achievement updated successfully!";
    } else {
        $response["status"] = "error";
        $response["message"] = "Failed to update achievement: " . $stmt->error;
    }
} else {
    // Insert new achievement if it doesn't exist
    $stmt = $pdo->prepare("INSERT INTO player_achievements (playerID, achievementID, isCompleted, completionDate) VALUES (?, ?, ?, ?)");
    if ($stmt->execute([$playerID, $achievementID, $isCompleted, $completionDate])) {
        $response["status"] = "success";
        $response["action"] = "inserted";
        $response["message"] = "Achievement inserted successfully!";
    } else {
        $response["status"] = "error";
        $response["message"] = "Failed to insert achievement: " . $stmt->error;
    }
}

echo json_encode($response);
?>