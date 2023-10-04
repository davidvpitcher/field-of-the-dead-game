<?php // retrieve-skin.inc.php
if (isset($_POST["username"])) {
    $username = $_POST["username"];
    $login= true;
    $unity = true;
    require_once '../../keys/storage.php';

    // Initialize response array
    $response = [];

    try {
        // Get the player's ID
        $sql = "SELECT id FROM players WHERE username = :username";
        $stmt = $pdo->prepare($sql);
        $stmt->bindValue(':username', $username);
        $stmt->execute();
        $player_id = $stmt->fetchColumn();

        if (!$player_id) {
            $response["status"] = "error";
            $response["message"] = "Player not found.";
            echo json_encode($response);
            exit();
        }

        // Get the player's current skin
        $sql = "SELECT current_skin FROM player_stats WHERE player_id = :player_id";
        $stmt = $pdo->prepare($sql);
        $stmt->bindValue(':player_id', $player_id);
        $stmt->execute();
        $current_skin = $stmt->fetchColumn();

        if ($current_skin === false) {
            // No row found, return special value
            echo "255";
        } else {
            echo $current_skin; // Return the current skin
        }
    } catch (PDOException $e) {
        $response["status"] = "error";
        $response["message"] = "Database error: " . $e->getMessage();
        echo json_encode($response);
        exit();
    }
} else {
    $response["status"] = "error";
    $response["message"] = "Missing username";
    echo json_encode($response);
    exit();
}
?>
