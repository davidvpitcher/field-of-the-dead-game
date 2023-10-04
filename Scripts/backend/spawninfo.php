<?php // spawninfo.php

if (isset($_POST["BASICCHECK"])) {
    $varvar = $_POST["BASICCHECK"];
    $unity = true;
    require_once '../../keys/storage.php';

    try {
        $intthing = intval($varvar);
        $intthingminus = $intthing - 1;
        
        $sql1 = 'SELECT * FROM `the_builded` LIMIT 1 OFFSET ?;';
        $stmt = $pdo->prepare($sql1);
        $stmt->bindParam(1, $intthingminus, PDO::PARAM_INT);
        $stmt->execute();
        
        $arrayofresults = $stmt->fetch();
        
        echo "0\t" . implode("\t", array_values($arrayofresults));
        
    } catch (Exception $e) {
        echo "SERVER_ERROR: " . $e->getMessage();
    }
    
    exit();
} else {
    echo "7: uh oh";
    exit();
}
?>
