<?php
// Constants for error messages
const ERROR_NOT_FOUND = "NOT_FOUND";
const ERROR_INVALID_REQUEST = "INVALID_REQUEST";
const ERROR_EXECUTION_FAILED = "Error: Execution failed.";

$unity = true;
$login = true;
require_once '../../keys/storage.php';

if (isset($_POST["action"], $_POST["username"]) && $_POST["action"] === "load") {
    try {
        // Set error handling for PDO
        $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
        
        $player_name = $_POST["username"];
        $sql = "SELECT players.id, player_quests.highest_completed_questdata
                FROM players
                LEFT JOIN player_quests ON players.id = player_quests.player_id
                WHERE players.username = :username";
        
        $stmt = $pdo->prepare($sql);
        $stmt->bindValue(':username', $player_name, PDO::PARAM_STR);
        $stmt->execute();
        
        $result = $stmt->fetch(PDO::FETCH_ASSOC);
        
        if ($result) {
            echo $result["highest_completed_questdata"];
        } else {
            echo ERROR_NOT_FOUND;
        }
    } catch (PDOException $e) {
        // Log the exception for debugging
        error_log("Database error: " . $e->getMessage());
        echo ERROR_EXECUTION_FAILED;
    }
} else {
    echo ERROR_INVALID_REQUEST;
}
exit();
?>
