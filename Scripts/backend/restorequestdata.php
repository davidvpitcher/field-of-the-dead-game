<?php // restorequestdata.php
// Constants for error messages
const ERROR_INVALID_REQUEST = "INVALID_REQUEST";
const ERROR_EXECUTION_FAILED = "Error: Execution failed.";

$unity = true;
$login = true;
require_once '../../keys/storage.php';

if (isset($_POST["action"], $_POST["username"]) && $_POST["action"] === "restore") {
    try {
        // Enable PDO exception mode
        $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

        $player_name = $_POST["username"];
        $sql = "SELECT id FROM players WHERE username = :username";
        $stmt = $pdo->prepare($sql);
        $stmt->bindParam(':username', $player_name, PDO::PARAM_STR);
        $stmt->execute();

        $player_id = $stmt->fetchColumn();

        if (!$player_id) {
            throw new Exception("Player not found");
        }

        $sql = "SELECT highest_completed_questdata FROM player_quests WHERE player_id = :player_id";
        $stmt = $pdo->prepare($sql);
        $stmt->bindParam(':player_id', $player_id, PDO::PARAM_INT);
        $stmt->execute();

        $result = $stmt->fetch(PDO::FETCH_ASSOC);

        if ($result) {
            $highest_completed_questdata = $result["highest_completed_questdata"];
            $sql = "UPDATE player_quests SET questdata = :questdata WHERE player_id = :player_id";
            $stmt = $pdo->prepare($sql);
            $stmt->bindParam(':questdata', $highest_completed_questdata, PDO::PARAM_STR);
            $stmt->bindParam(':player_id', $player_id, PDO::PARAM_INT);
            $stmt->execute();
            echo $highest_completed_questdata;
        } else {
            $default_quest_data = '000000000000000000000000';
            $sql = "INSERT INTO player_quests (player_id, questdata, highest_completed_questdata) VALUES (:player_id, :questdata, :highest_completed_questdata)";
            $stmt = $pdo->prepare($sql);
            $stmt->bindParam(':player_id', $player_id, PDO::PARAM_INT);
            $stmt->bindParam(':questdata', $default_quest_data, PDO::PARAM_STR);
            $stmt->bindParam(':highest_completed_questdata', $default_quest_data, PDO::PARAM_STR);
            $stmt->execute();
            echo $default_quest_data;
        }
    } catch (PDOException $e) {
        error_log("Database error: " . $e->getMessage());
        echo ERROR_EXECUTION_FAILED;
    } catch (Exception $e) {
        error_log("General error: " . $e->getMessage());
        echo ERROR_EXECUTION_FAILED;
    }
} else {
    echo ERROR_INVALID_REQUEST;
}
exit();
?>
