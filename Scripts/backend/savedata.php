<?php // savedata.php

$unity = true;
$saving = true;

require_once '../../keys/storage.php';

if (!isset($_POST["score"]) || !isset($_POST["scoreusername"])) {
    error_log("Missing POST data");
    exit();
}

$username = $_POST["scoreusername"];
$newscore = $_POST["score"];

// Prepare statement to check username
$stmt = $conn->prepare("SELECT username FROM players WHERE username = ?");
$stmt->bind_param('s', $username);
$stmt->execute();
$result = $stmt->get_result();

if ($result->num_rows != 1) {
    echo "5: either no user with name or more than 1";
    exit();
}

// Prepare statement to update the score
$stmt = $conn->prepare("UPDATE players SET kills = ? WHERE username = ?");
$stmt->bind_param('is', $newscore, $username);

if ($stmt->execute() === TRUE) {
    echo "0";
} else {
    echo "7: SAVE query FAILED";
}

$conn->close();
?>
