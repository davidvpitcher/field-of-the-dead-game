<?php // savefunds.php

$unity = true;
$saving = true;

require_once '../../keys/storage.php';

if (isset($_POST["funds"]) && isset($_POST["fundsusername"])) {
    $username = $_POST["fundsusername"];
    $newscore = $_POST["funds"];
    
    // Prepare and execute name check query
    $stmt = $conn->prepare("SELECT username FROM players WHERE username = ?");
    $stmt->bind_param('s', $username);
    $stmt->execute();
    $result = $stmt->get_result();
    
    if ($result->num_rows != 1) {
        echo "5: either no user with name or more than 1";
        exit();
    }
    
    // Update funds for the player
    $updateStmt = $conn->prepare("UPDATE players SET funds = ? WHERE username = ?");
    $updateStmt->bind_param('is', $newscore, $username);
    $updateStmt->execute();
    
    if ($updateStmt->affected_rows == 0) {
        echo "6: No funds updated";
        exit();
    }
    
    echo "0";
    exit();
} else {
    echo "7: Missing fields";
    exit();
}
?>
